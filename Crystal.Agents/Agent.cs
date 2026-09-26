using System.Runtime.CompilerServices;

using Crystal.Chat;
using Crystal.Internal;
using Crystal.Tools;

namespace Crystal.Agents;

/// <summary>
/// Runs an explicit, prompt-free text model and tool loop with optional limits.
/// </summary>
public sealed class Agent : IAgent
{
    private readonly ChatCandidateSelector _candidateSelector;
    private readonly IChatClient _client;
    private readonly IStreamingChatClient? _streamingClient;
    private readonly IToolExecutor? _toolExecutor;

    /// <summary>
    /// Initializes an Agent.
    /// </summary>
    /// <param name="client">The provider-neutral text-chat client.</param>
    /// <param name="candidateSelector">
    /// The caller-owned candidate-selection policy.
    /// </param>
    /// <param name="toolExecutor">
    /// An optional explicitly configured tool executor.
    /// </param>
    public Agent(
        IChatClient client,
        ChatCandidateSelector candidateSelector,
        IToolExecutor? toolExecutor = null)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        ArgumentNullException.ThrowIfNull(
            candidateSelector,
            nameof(candidateSelector));

        _client = client;
        _streamingClient = client as IStreamingChatClient;
        _candidateSelector = candidateSelector;
        _toolExecutor = toolExecutor;
    }

    /// <inheritdoc />
    public async Task<AgentRunResult> RunAsync(
        AgentRunRequest request,
        CancellationToken cancellationToken = default)
    {
        AgentRunResult? result = null;

        await foreach (var runEvent in StreamAsync(
                request,
                cancellationToken)
            .ConfigureAwait(false))
        {
            if (runEvent is AgentRunCompletedEvent completedEvent)
            {
                result = completedEvent.Result;
            }
        }

        return result
            ?? throw new InvalidOperationException(
                "The Agent event stream ended without a completion result.");
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<AgentRunEvent> StreamAsync(
        AgentRunRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        using var durationSource = request.Limits.MaximumDuration is TimeSpan duration
            ? new CancellationTokenSource(duration)
            : new CancellationTokenSource();
        using var operationSource =
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                durationSource.Token);

        var transcript = new List<ChatItem>(request.Items);
        var usage = new TokenUsageAccumulator();
        var modelCallCount = 0;
        var toolCallCount = 0;
        long sequence = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (durationSource.IsCancellationRequested)
            {
                var durationResult = CreateResult(
                    request,
                    transcript,
                    AgentRunStopReason.DurationLimitReached,
                    modelCallCount,
                    toolCallCount,
                    usage);

                yield return new AgentRunCompletedEvent(
                    request.RunId,
                    sequence,
                    durationResult);
                yield break;
            }

            if (request.Limits.MaximumModelCalls is int maximumModelCalls
                && modelCallCount >= maximumModelCalls)
            {
                var modelLimitResult = CreateResult(
                    request,
                    transcript,
                    AgentRunStopReason.ModelCallLimitReached,
                    modelCallCount,
                    toolCallCount,
                    usage);

                yield return new AgentRunCompletedEvent(
                    request.RunId,
                    sequence,
                    modelLimitResult);
                yield break;
            }

            var chatRequest = new ChatRequest(
                transcript,
                _toolExecutor?.Definitions,
                request.Reasoning);
            modelCallCount = checked(modelCallCount + 1);

            yield return new AgentModelRequestEvent(
                request.RunId,
                sequence++,
                modelCallCount,
                chatRequest);
            cancellationToken.ThrowIfCancellationRequested();
            if (durationSource.IsCancellationRequested)
            {
                usage.Add(null);
                yield return new AgentRunCompletedEvent(
                    request.RunId,
                    sequence,
                    CreateResult(
                        request,
                        transcript,
                        AgentRunStopReason.DurationLimitReached,
                        modelCallCount,
                        toolCallCount,
                        usage));
                yield break;
            }

            ChatResponse response;
            if (_streamingClient is null)
            {
                var modelOperation = await ExecuteOperationAsync(
                        token => _client.CompleteAsync(chatRequest, token),
                        cancellationToken,
                        durationSource.Token,
                        operationSource.Token)
                    .ConfigureAwait(false);

                if (modelOperation.TimedOut)
                {
                    usage.Add(null);
                    yield return new AgentRunCompletedEvent(
                        request.RunId,
                        sequence,
                        CreateResult(
                            request,
                            transcript,
                            AgentRunStopReason.DurationLimitReached,
                            modelCallCount,
                            toolCallCount,
                            usage));
                    yield break;
                }

                response = modelOperation.Value
                    ?? throw new InvalidOperationException(
                        "The chat client returned no response.");
            }
            else
            {
                var assembler = new ChatStreamAssembler();
                var timedOut = false;
                var enumerator = _streamingClient
                    .StreamAsync(chatRequest, operationSource.Token)
                    .GetAsyncEnumerator(operationSource.Token);

                try
                {
                    while (true)
                    {
                        var moveOperation = await ExecuteMoveNextAsync(
                                enumerator,
                                cancellationToken,
                                durationSource.Token)
                            .ConfigureAwait(false);

                        if (moveOperation.TimedOut)
                        {
                            timedOut = true;
                            break;
                        }

                        if (!moveOperation.Value)
                        {
                            break;
                        }

                        var streamEvent = enumerator.Current
                            ?? throw new InvalidOperationException(
                                "The Chat client streamed no event value.");
                        assembler.Apply(streamEvent);

                        yield return new AgentModelStreamEvent(
                            request.RunId,
                            sequence++,
                            modelCallCount,
                            streamEvent);
                    }
                }
                finally
                {
                    var disposeOperation = await ExecuteDisposeAsync(
                            enumerator,
                            cancellationToken,
                            durationSource.Token)
                        .ConfigureAwait(false);
                    timedOut |= disposeOperation.TimedOut;
                }

                cancellationToken.ThrowIfCancellationRequested();
                timedOut |= durationSource.IsCancellationRequested;

                if (timedOut)
                {
                    usage.Add(null);
                    yield return new AgentRunCompletedEvent(
                        request.RunId,
                        sequence,
                        CreateResult(
                            request,
                            transcript,
                            AgentRunStopReason.DurationLimitReached,
                            modelCallCount,
                            toolCallCount,
                            usage));
                    yield break;
                }

                response = assembler.ToResponse();
            }

            usage.Add(response.Usage);

            yield return new AgentModelResponseEvent(
                request.RunId,
                sequence++,
                modelCallCount,
                response);

            var selectionOperation = await ExecuteOperationAsync(
                    token => _candidateSelector(response, token).AsTask(),
                    cancellationToken,
                    durationSource.Token,
                    operationSource.Token)
                .ConfigureAwait(false);

            if (selectionOperation.TimedOut)
            {
                var durationResult = CreateResult(
                    request,
                    transcript,
                    AgentRunStopReason.DurationLimitReached,
                    modelCallCount,
                    toolCallCount,
                    usage);

                yield return new AgentRunCompletedEvent(
                    request.RunId,
                    sequence,
                    durationResult);
                yield break;
            }

            var selectedCandidateIndex = selectionOperation.Value;

            if (selectedCandidateIndex < 0
                || selectedCandidateIndex >= response.Candidates.Count)
            {
                throw new InvalidOperationException(
                    "The candidate selector returned an out-of-range index.");
            }

            var candidate = response.Candidates[selectedCandidateIndex];
            transcript.AddRange(candidate.Items);

            yield return new AgentCandidateSelectedEvent(
                request.RunId,
                sequence++,
                modelCallCount,
                selectedCandidateIndex,
                candidate);
            cancellationToken.ThrowIfCancellationRequested();
            if (durationSource.IsCancellationRequested)
            {
                yield return new AgentRunCompletedEvent(
                    request.RunId,
                    sequence,
                    CreateResult(
                        request,
                        transcript,
                        AgentRunStopReason.DurationLimitReached,
                        modelCallCount,
                        toolCallCount,
                        usage));
                yield break;
            }

            var toolCalls = candidate.Items
                .OfType<ToolCall>()
                .ToArray();

            if (toolCalls.Length == 0)
            {
                var completedResult = CreateResult(
                    request,
                    transcript,
                    AgentRunStopReason.Completed,
                    modelCallCount,
                    toolCallCount,
                    usage,
                    candidate.FinishReason);

                yield return new AgentRunCompletedEvent(
                    request.RunId,
                    sequence,
                    completedResult);
                yield break;
            }

            if (request.Limits.MaximumToolCalls is int maximumToolCalls
                && toolCalls.Length > maximumToolCalls - toolCallCount)
            {
                var toolLimitResult = CreateResult(
                    request,
                    transcript,
                    AgentRunStopReason.ToolCallLimitReached,
                    modelCallCount,
                    toolCallCount,
                    usage);

                yield return new AgentRunCompletedEvent(
                    request.RunId,
                    sequence,
                    toolLimitResult);
                yield break;
            }

            if (_toolExecutor is null)
            {
                throw new InvalidOperationException(
                    "The selected candidate requested tools, but no tool executor is configured.");
            }

            toolCallCount = checked(toolCallCount + toolCalls.Length);
            var callSnapshot = Array.AsReadOnly(toolCalls);

            yield return new AgentToolExecutionStartedEvent(
                request.RunId,
                sequence++,
                modelCallCount,
                callSnapshot);

            var toolOperation = await ExecuteOperationAsync(
                    token => _toolExecutor.ExecuteAsync(callSnapshot, token),
                    cancellationToken,
                    durationSource.Token,
                    operationSource.Token)
                .ConfigureAwait(false);

            if (toolOperation.TimedOut)
            {
                var durationResult = CreateResult(
                    request,
                    transcript,
                    AgentRunStopReason.DurationLimitReached,
                    modelCallCount,
                    toolCallCount,
                    usage);

                yield return new AgentRunCompletedEvent(
                    request.RunId,
                    sequence,
                    durationResult);
                yield break;
            }

            var toolResults = SnapshotAndValidateToolResults(
                callSnapshot,
                toolOperation.Value);
            transcript.AddRange(toolResults);

            yield return new AgentToolExecutionCompletedEvent(
                request.RunId,
                sequence++,
                modelCallCount,
                toolResults);
        }
    }

    private static AgentRunResult CreateResult(
        AgentRunRequest request,
        IEnumerable<ChatItem> transcript,
        AgentRunStopReason stopReason,
        int modelCallCount,
        int toolCallCount,
        TokenUsageAccumulator usage,
        FinishReason? finishReason = null) =>
        new(
            request.RunId,
            transcript,
            stopReason,
            modelCallCount,
            toolCallCount,
            usage.Build(),
            finishReason);

    private static async Task<AgentOperationResult<T>> ExecuteOperationAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken callerToken,
        CancellationToken durationToken,
        CancellationToken operationToken)
    {
        try
        {
            callerToken.ThrowIfCancellationRequested();
            if (durationToken.IsCancellationRequested)
            {
                return AgentOperationResult<T>.Timeout();
            }

            var value = await operation(operationToken).ConfigureAwait(false);
            callerToken.ThrowIfCancellationRequested();
            if (durationToken.IsCancellationRequested)
            {
                return AgentOperationResult<T>.Timeout();
            }

            return AgentOperationResult<T>.Success(value);
        }
        catch (OperationCanceledException)
            when (!callerToken.IsCancellationRequested
                && durationToken.IsCancellationRequested)
        {
            return AgentOperationResult<T>.Timeout();
        }
    }

    private static async Task<AgentOperationResult<bool>> ExecuteMoveNextAsync(
        IAsyncEnumerator<ChatStreamEvent> enumerator,
        CancellationToken callerToken,
        CancellationToken durationToken)
    {
        try
        {
            callerToken.ThrowIfCancellationRequested();
            if (durationToken.IsCancellationRequested)
            {
                return AgentOperationResult<bool>.Timeout();
            }

            var hasNext = await enumerator.MoveNextAsync().ConfigureAwait(false);
            callerToken.ThrowIfCancellationRequested();
            if (durationToken.IsCancellationRequested)
            {
                return AgentOperationResult<bool>.Timeout();
            }

            return AgentOperationResult<bool>.Success(hasNext);
        }
        catch (OperationCanceledException)
            when (!callerToken.IsCancellationRequested
                && durationToken.IsCancellationRequested)
        {
            return AgentOperationResult<bool>.Timeout();
        }
    }

    private static async Task<AgentOperationResult<bool>> ExecuteDisposeAsync(
        IAsyncEnumerator<ChatStreamEvent> enumerator,
        CancellationToken callerToken,
        CancellationToken durationToken)
    {
        try
        {
            await enumerator.DisposeAsync().ConfigureAwait(false);
            return AgentOperationResult<bool>.Success(true);
        }
        catch (OperationCanceledException)
            when (!callerToken.IsCancellationRequested
                && durationToken.IsCancellationRequested)
        {
            return AgentOperationResult<bool>.Timeout();
        }
    }

    private static IReadOnlyList<ToolResult> SnapshotAndValidateToolResults(
        IReadOnlyList<ToolCall> calls,
        IReadOnlyList<ToolResult>? results)
    {
        if (results is null)
        {
            throw new InvalidOperationException(
                "The tool executor returned no result collection.");
        }

        if (results.Count != calls.Count)
        {
            throw new InvalidOperationException(
                "The tool executor returned an unexpected result count.");
        }

        var snapshot = new ToolResult[results.Count];

        for (var index = 0; index < results.Count; index++)
        {
            var result = results[index]
                ?? throw new InvalidOperationException(
                    "The tool executor returned a null result.");

            if (!string.Equals(
                    calls[index].CallId,
                    result.CallId,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "The tool executor returned results out of correlation order.");
            }

            snapshot[index] = result;
        }

        return Array.AsReadOnly(snapshot);
    }
}
