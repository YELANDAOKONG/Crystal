using System.Runtime.CompilerServices;

using Crystal.Workflows.Internal;

namespace Crystal.Workflows;

/// <summary>Runs an immutable typed graph without owning application state.</summary>
/// <typeparam name="TInput">The graph input type.</typeparam>
/// <typeparam name="TOutput">The terminal output type.</typeparam>
public sealed class Workflow<TInput, TOutput>
    where TInput : notnull
    where TOutput : notnull
{
    private readonly IWorkflowNode _start;
    private readonly IWorkflowNode _terminal;
    private readonly IReadOnlyList<IWorkflowNode> _nodes;
    private readonly IReadOnlyList<IWorkflowEdge> _edges;

    internal Workflow(
        IWorkflowNode start,
        IWorkflowNode terminal,
        IReadOnlyList<IWorkflowNode> nodes,
        IReadOnlyList<IWorkflowEdge> edges)
    {
        _start = start;
        _terminal = terminal;
        _nodes = nodes;
        _edges = edges;
    }

    /// <summary>Runs until routing completes or the configured step limit stops it.</summary>
    /// <param name="request">The exact caller-authored invocation.</param>
    /// <param name="cancellationToken">Cancels node and route work.</param>
    /// <returns>Ordered terminal output and execution accounting.</returns>
    public async Task<WorkflowRunResult<TOutput>> RunAsync(
        WorkflowRunRequest<TInput> request,
        CancellationToken cancellationToken = default)
    {
        WorkflowRunResult<TOutput>? result = null;
        await foreach (var runEvent in StreamAsync(request, cancellationToken)
            .ConfigureAwait(false))
        {
            if (runEvent is WorkflowCompletedEvent<TOutput> completed)
            {
                result = completed.Result;
            }
        }

        return result
            ?? throw new InvalidOperationException(
                "The workflow stream ended without a completion result.");
    }

    /// <summary>Streams ordered metadata events and one terminal result event.</summary>
    /// <param name="request">The exact caller-authored invocation.</param>
    /// <param name="cancellationToken">Cancels enumeration and in-flight work.</param>
    /// <returns>Ordered workflow transitions.</returns>
    public async IAsyncEnumerable<WorkflowEvent> StreamAsync(
        WorkflowRunRequest<TInput> request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var pending = new Dictionary<IWorkflowNode, List<object>>
        {
            [_start] = request.Inputs.Cast<object>().ToList()
        };
        var terminalOutputs = new List<TOutput>();
        var superstepCount = 0;
        var nodeCallCount = 0;
        long sequence = 0;

        while (pending.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (request.Limits.MaximumSupersteps is int maximumSupersteps
                && superstepCount >= maximumSupersteps)
            {
                var limitResult = CreateResult(
                    request,
                    terminalOutputs,
                    WorkflowStopReason.SuperstepLimitReached,
                    superstepCount,
                    nodeCallCount);
                yield return new WorkflowCompletedEvent<TOutput>(
                    request.RunId,
                    sequence,
                    superstepCount,
                    limitResult);
                yield break;
            }

            var active = _nodes.Where(pending.ContainsKey).ToArray();
            var nodeOutputs = new IReadOnlyList<object>[active.Length];
            var options = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = request.Limits.MaximumConcurrency
            };
            nodeCallCount = checked(nodeCallCount + active.Length);

            await Parallel.ForEachAsync(
                    Enumerable.Range(0, active.Length),
                    options,
                    async (index, token) =>
                    {
                        token.ThrowIfCancellationRequested();
                        var inputs = Array.AsReadOnly(
                            pending[active[index]].ToArray());
                        nodeOutputs[index] = await active[index]
                            .InvokeAsync(inputs, token)
                            .ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                    })
                .ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
            superstepCount = checked(superstepCount + 1);
            var next = new Dictionary<IWorkflowNode, List<object>>();

            for (var nodeIndex = 0; nodeIndex < active.Length; nodeIndex++)
            {
                var node = active[nodeIndex];
                var outputs = nodeOutputs[nodeIndex];

                yield return new WorkflowNodeCompletedEvent(
                    request.RunId,
                    sequence++,
                    superstepCount,
                    node.Name,
                    pending[node].Count,
                    outputs.Count);
                cancellationToken.ThrowIfCancellationRequested();

                if (node == _terminal)
                {
                    foreach (var output in outputs)
                    {
                        terminalOutputs.Add((TOutput)output);
                    }

                    continue;
                }

                for (var outputIndex = 0; outputIndex < outputs.Count; outputIndex++)
                {
                    var output = outputs[outputIndex];
                    foreach (var edge in _edges)
                    {
                        if (edge.Source != node)
                        {
                            continue;
                        }

                        cancellationToken.ThrowIfCancellationRequested();
                        var accepted = await edge.AcceptsAsync(
                                output, cancellationToken)
                            .ConfigureAwait(false);
                        cancellationToken.ThrowIfCancellationRequested();

                        if (!accepted)
                        {
                            continue;
                        }

                        if (!next.TryGetValue(edge.Target, out var targetInputs))
                        {
                            targetInputs = [];
                            next.Add(edge.Target, targetInputs);
                        }

                        targetInputs.Add(output);
                        yield return new WorkflowMessageRoutedEvent(
                            request.RunId,
                            sequence++,
                            superstepCount,
                            node.Name,
                            edge.Target.Name,
                            outputIndex);
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
            }

            pending = next;
        }

        cancellationToken.ThrowIfCancellationRequested();
        var result = CreateResult(
            request,
            terminalOutputs,
            WorkflowStopReason.Completed,
            superstepCount,
            nodeCallCount);
        yield return new WorkflowCompletedEvent<TOutput>(
            request.RunId,
            sequence,
            superstepCount,
            result);
    }

    private static WorkflowRunResult<TOutput> CreateResult(
        WorkflowRunRequest<TInput> request,
        IEnumerable<TOutput> outputs,
        WorkflowStopReason stopReason,
        int superstepCount,
        int nodeCallCount) =>
        new(
            request.RunId,
            outputs,
            stopReason,
            superstepCount,
            nodeCallCount);
}
