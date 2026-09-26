namespace Crystal.Pipelines;

/// <summary>Wraps one asynchronous event stream with caller-supplied behavior.</summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TEvent">The event type.</typeparam>
/// <param name="next">The next stream operation in the pipeline.</param>
/// <returns>The wrapped stream operation.</returns>
public delegate StreamingOperation<TRequest, TEvent> StreamingMiddleware<TRequest, TEvent>(
    StreamingOperation<TRequest, TEvent> next);
