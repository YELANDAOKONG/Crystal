using Crystal.Internal;
using Crystal.Tools;

namespace Crystal.Multimodal.Tools;

/// <summary>
/// Executes caller-owned multimodal tools under explicit scheduling and
/// disclosure policies.
/// </summary>
public sealed class MultimodalToolExecutor : IMultimodalToolExecutor
{
    private readonly MultimodalToolCatalog _catalog;
    private readonly MultimodalToolExceptionMapper? _exceptionMapper;
    private readonly MultimodalToolInvocationPolicy? _invocationPolicy;
    private readonly MultimodalToolExecutionOptions _options;

    /// <summary>Initializes a multimodal tool executor.</summary>
    /// <param name="catalog">The immutable registered-tool catalog.</param>
    /// <param name="options">The explicit scheduling options.</param>
    /// <param name="invocationPolicy">
    /// An optional caller-owned pre-invocation policy.
    /// </param>
    /// <param name="exceptionMapper">
    /// An optional caller-owned exception-to-output mapper.
    /// </param>
    public MultimodalToolExecutor(
        MultimodalToolCatalog catalog,
        MultimodalToolExecutionOptions options,
        MultimodalToolInvocationPolicy? invocationPolicy = null,
        MultimodalToolExceptionMapper? exceptionMapper = null)
    {
        ArgumentNullException.ThrowIfNull(catalog, nameof(catalog));
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        _catalog = catalog;
        _options = options;
        _invocationPolicy = invocationPolicy;
        _exceptionMapper = exceptionMapper;
    }

    /// <inheritdoc />
    public IReadOnlyList<ToolDefinition> Definitions => _catalog.Definitions;

    /// <inheritdoc />
    public async Task<IReadOnlyList<MultimodalToolResult>> ExecuteAsync(
        IEnumerable<MultimodalToolCall> calls,
        CancellationToken cancellationToken = default)
    {
        var snapshot = CollectionSnapshot.Create(calls, nameof(calls));

        return _options.Mode switch
        {
            MultimodalToolExecutionMode.Serial =>
                await ExecuteSerialAsync(snapshot, cancellationToken)
                    .ConfigureAwait(false),
            MultimodalToolExecutionMode.Concurrent =>
                await ExecuteConcurrentAsync(snapshot, cancellationToken)
                    .ConfigureAwait(false),
            _ => throw new InvalidOperationException(
                "The configured multimodal tool execution mode is invalid.")
        };
    }

    private async Task<IReadOnlyList<MultimodalToolResult>> ExecuteSerialAsync(
        IReadOnlyList<MultimodalToolCall> calls,
        CancellationToken cancellationToken)
    {
        var results = new MultimodalToolResult[calls.Count];

        for (var index = 0; index < calls.Count; index++)
        {
            results[index] = await ExecuteCallAsync(
                    calls[index],
                    cancellationToken)
                .ConfigureAwait(false);
        }

        return Array.AsReadOnly(results);
    }

    private async Task<IReadOnlyList<MultimodalToolResult>>
        ExecuteConcurrentAsync(
            IReadOnlyList<MultimodalToolCall> calls,
            CancellationToken cancellationToken)
    {
        using var concurrencyGate = new SemaphoreSlim(
            _options.MaximumConcurrency,
            _options.MaximumConcurrency);
        var tasks = new Task<MultimodalToolResult>[calls.Count];

        for (var index = 0; index < calls.Count; index++)
        {
            tasks[index] = ExecuteWithGateAsync(
                calls[index],
                concurrencyGate,
                cancellationToken);
        }

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return Array.AsReadOnly(results);
    }

    private async Task<MultimodalToolResult> ExecuteWithGateAsync(
        MultimodalToolCall call,
        SemaphoreSlim concurrencyGate,
        CancellationToken cancellationToken)
    {
        await concurrencyGate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            return await ExecuteCallAsync(call, cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            concurrencyGate.Release();
        }
    }

    private async Task<MultimodalToolResult> ExecuteCallAsync(
        MultimodalToolCall call,
        CancellationToken cancellationToken)
    {
        var tool = _catalog.Find(call.Name)
            ?? throw new MultimodalToolNotFoundException();

        var decision = await GetInvocationDecisionAsync(
                call,
                cancellationToken)
            .ConfigureAwait(false);

        switch (decision.Action)
        {
            case MultimodalToolInvocationAction.Execute:
                var output = await InvokeToolAsync(
                        tool,
                        call,
                        cancellationToken)
                    .ConfigureAwait(false);
                return CreateResult(call, output);
            case MultimodalToolInvocationAction.Reject:
                var rejectionOutput = decision.RejectionOutput
                    ?? throw new MultimodalToolInvocationRejectedException();
                return CreateResult(call, rejectionOutput);
            default:
                throw new InvalidOperationException(
                    "The multimodal tool policy returned an invalid action.");
        }
    }

    private async ValueTask<MultimodalToolOutput> InvokeToolAsync(
        IMultimodalTool tool,
        MultimodalToolCall call,
        CancellationToken cancellationToken)
    {
        MultimodalToolOutput? output;

        try
        {
            output = await tool.InvokeAsync(call, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            if (_exceptionMapper is null)
            {
                throw new MultimodalToolInvocationException(exception);
            }

            output = await _exceptionMapper(
                    call,
                    exception,
                    cancellationToken)
                .ConfigureAwait(false);

            if (output is null)
            {
                throw new MultimodalToolInvocationException(exception);
            }
        }

        if (output is null)
        {
            throw new InvalidOperationException(
                "A registered multimodal tool returned no output.");
        }

        return output;
    }

    private async ValueTask<MultimodalToolInvocationDecision>
        GetInvocationDecisionAsync(
            MultimodalToolCall call,
            CancellationToken cancellationToken)
    {
        if (_invocationPolicy is null)
        {
            return MultimodalToolInvocationDecision.Execute;
        }

        var decision = await _invocationPolicy(call, cancellationToken)
            .ConfigureAwait(false);

        if (decision is null)
        {
            throw new InvalidOperationException(
                "The multimodal tool policy returned no decision.");
        }

        return decision;
    }

    private static MultimodalToolResult CreateResult(
        MultimodalToolCall call,
        MultimodalToolOutput output) =>
        new(call.CallId, output.Contents, output.Status);
}
