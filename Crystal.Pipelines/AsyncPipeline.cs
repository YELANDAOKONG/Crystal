namespace Crystal.Pipelines;

/// <summary>Composes caller-owned middleware around one asynchronous operation.</summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class AsyncPipeline<TRequest, TResponse>
{
    private readonly AsyncOperation<TRequest, TResponse> _operation;

    /// <summary>Creates a pipeline. Middleware runs in the order supplied.</summary>
    /// <param name="terminal">The operation reached after all middleware.</param>
    /// <param name="middleware">Ordered caller-owned middleware.</param>
    public AsyncPipeline(
        AsyncOperation<TRequest, TResponse> terminal,
        IEnumerable<AsyncMiddleware<TRequest, TResponse>> middleware)
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
                    "Middleware must return an operation.",
                    nameof(middleware));
        }

        _operation = operation;
    }

    /// <summary>Invokes the pipeline without changing the supplied request.</summary>
    /// <param name="request">The caller-supplied request.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>The response returned by caller-owned middleware or the terminal.</returns>
    public Task<TResponse> InvokeAsync(
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        return _operation(request, cancellationToken);
    }
}
