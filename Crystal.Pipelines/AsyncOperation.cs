namespace Crystal.Pipelines;

/// <summary>Represents one asynchronous operation with an explicit request and response.</summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
/// <param name="request">The request.</param>
/// <param name="cancellationToken">Cancels the operation.</param>
/// <returns>The response.</returns>
public delegate Task<TResponse> AsyncOperation<TRequest, TResponse>(
    TRequest request,
    CancellationToken cancellationToken);
