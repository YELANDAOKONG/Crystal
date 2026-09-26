namespace Crystal.Pipelines;

/// <summary>Composes caller-owned middleware around one asynchronous event stream.</summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TEvent">The event type.</typeparam>
public sealed class StreamingPipeline<TRequest, TEvent>
{
    private readonly StreamingOperation<TRequest, TEvent> _operation;

    /// <summary>Creates a stream pipeline. Middleware runs in the order supplied.</summary>
    /// <param name="terminal">The stream source reached after all middleware.</param>
    /// <param name="middleware">Ordered caller-owned middleware.</param>
    public StreamingPipeline(
        StreamingOperation<TRequest, TEvent> terminal,
        IEnumerable<StreamingMiddleware<TRequest, TEvent>> middleware)
    {
        ArgumentNullException.ThrowIfNull(terminal, nameof(terminal));
        ArgumentNullException.ThrowIfNull(middleware, nameof(middleware));

        var steps = middleware.ToArray();
        if (steps.Any(static step => step is null))
        {
            throw new ArgumentException(
                "Middleware cannot contain a null entry.",
                nameof(middleware));
        }

        var operation = terminal;
        for (var index = steps.Length - 1; index >= 0; index--)
        {
            operation = steps[index](operation)
                ?? throw new ArgumentException(
                    "Middleware must return a stream operation.",
                    nameof(middleware));
        }

        _operation = operation;
    }

    /// <summary>Starts the caller-owned stream operation.</summary>
    /// <param name="request">The caller-supplied request.</param>
    /// <param name="cancellationToken">Cancels stream enumeration and source work.</param>
    /// <returns>The ordered stream from middleware or the terminal.</returns>
    public IAsyncEnumerable<TEvent> StreamAsync(
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        return _operation(request, cancellationToken);
    }
}
