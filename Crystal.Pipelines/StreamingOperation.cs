namespace Crystal.Pipelines;

/// <summary>Represents one asynchronous stream operation.</summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TEvent">The event type.</typeparam>
/// <param name="request">The request.</param>
/// <param name="cancellationToken">Cancels stream enumeration and source work.</param>
/// <returns>The ordered event stream.</returns>
public delegate IAsyncEnumerable<TEvent> StreamingOperation<TRequest, TEvent>(
    TRequest request,
    CancellationToken cancellationToken);
