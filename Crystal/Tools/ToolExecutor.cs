using Crystal.Internal;

namespace Crystal.Tools;

/// <summary>
/// Executes caller-owned tools under explicit scheduling and disclosure policies.
/// </summary>
public sealed class ToolExecutor : IToolExecutor
{
    private readonly ToolCatalog _catalog;
    private readonly ToolExceptionMapper? _exceptionMapper;
    private readonly ToolInvocationPolicy? _invocationPolicy;
    private readonly ToolExecutionOptions _options;

    /// <summary>
    /// Initializes a tool executor.
    /// </summary>
    /// <param name="catalog">The immutable registered-tool catalog.</param>
    /// <param name="options">The explicit scheduling options.</param>
    /// <param name="invocationPolicy">
    /// An optional caller-owned pre-invocation policy.
    /// </param>
    /// <param name="exceptionMapper">
    /// An optional caller-owned exception-to-output mapper.
    /// </param>
    public ToolExecutor(
        ToolCatalog catalog,
        ToolExecutionOptions options,
        ToolInvocationPolicy? invocationPolicy = null,
        ToolExceptionMapper? exceptionMapper = null)
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
    public async Task<IReadOnlyList<ToolResult>> ExecuteAsync(
        IEnumerable<ToolCall> calls,
        CancellationToken cancellationToken = default)
    {
        var snapshot = CollectionSnapshot.Create(calls, nameof(calls));

        return _options.Mode switch
        {
            ToolExecutionMode.Serial =>
                await ExecuteSerialAsync(snapshot, cancellationToken)
                    .ConfigureAwait(false),
            ToolExecutionMode.Concurrent =>
                await ExecuteConcurrentAsync(snapshot, cancellationToken)
                    .ConfigureAwait(false),
            _ => throw new InvalidOperationException(
                "The configured tool execution mode is invalid.")
        };
    }

    private async Task<IReadOnlyList<ToolResult>> ExecuteSerialAsync(
        IReadOnlyList<ToolCall> calls,
        CancellationToken cancellationToken)
    {
        var results = new ToolResult[calls.Count];

        for (var index = 0; index < calls.Count; index++)
        {
            results[index] = await ExecuteOneAsync(
                    calls[index],
                    cancellationToken)
                .ConfigureAwait(false);
        }

        return Array.AsReadOnly(results);
    }

    private async Task<IReadOnlyList<ToolResult>> ExecuteConcurrentAsync(
        IReadOnlyList<ToolCall> calls,
        CancellationToken cancellationToken)
    {
        using var concurrencyGate = new SemaphoreSlim(
            _options.MaximumConcurrency,
            _options.MaximumConcurrency);
        var tasks = new Task<ToolResult>[calls.Count];

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

    private async Task<ToolResult> ExecuteWithGateAsync(
        ToolCall call,
        SemaphoreSlim concurrencyGate,
        CancellationToken cancellationToken)
    {
        await concurrencyGate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            return await ExecuteOneAsync(call, cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            concurrencyGate.Release();
        }
    }

    private async Task<ToolResult> ExecuteOneAsync(
        ToolCall call,
        CancellationToken cancellationToken)
    {
        var tool = _catalog.Find(call.Name)
            ?? throw new ToolNotFoundException();

        var policyResult = await EvaluatePolicyAsync(
                call,
                cancellationToken)
            .ConfigureAwait(false);

        if (policyResult is not null)
        {
            return CreateResult(call, policyResult);
        }

        ToolOutput? output;

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
                throw new ToolInvocationException(exception);
            }

            output = await _exceptionMapper(
                    call,
                    exception,
                    cancellationToken)
                .ConfigureAwait(false);

            if (output is null)
            {
                throw new ToolInvocationException(exception);
            }
        }

        if (output is null)
        {
            throw new InvalidOperationException(
                "A registered tool returned no output.");
        }

        return CreateResult(call, output);
    }

    private async ValueTask<ToolOutput?> EvaluatePolicyAsync(
        ToolCall call,
        CancellationToken cancellationToken)
    {
        if (_invocationPolicy is null)
        {
            return null;
        }

        var decision = await _invocationPolicy(call, cancellationToken)
            .ConfigureAwait(false);

        if (decision is null)
        {
            throw new InvalidOperationException(
                "The tool invocation policy returned no decision.");
        }

        return decision.Action switch
        {
            ToolInvocationAction.Execute => null,
            ToolInvocationAction.Reject =>
                decision.RejectionOutput
                ?? throw new ToolInvocationRejectedException(),
            _ => throw new InvalidOperationException(
                "The tool invocation policy returned an invalid action.")
        };
    }

    private static ToolResult CreateResult(
        ToolCall call,
        ToolOutput output) =>
        new(call.CallId, output.Text, output.Status);
}
