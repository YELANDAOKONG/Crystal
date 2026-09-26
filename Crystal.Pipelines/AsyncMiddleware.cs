namespace Crystal.Pipelines;

/// <summary>Wraps an asynchronous operation with caller-supplied behavior.</summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
/// <param name="next">The next operation in the pipeline.</param>
/// <returns>The wrapped operation.</returns>
public delegate AsyncOperation<TRequest, TResponse> AsyncMiddleware<TRequest, TResponse>(
    AsyncOperation<TRequest, TResponse> next);
