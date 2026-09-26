namespace Crystal.Pipelines;

/// <summary>Creates opt-in retries governed by caller-owned decisions.</summary>
public static class PipelineRetries
{
    /// <summary>Retries an operation only when the caller approves a failure.</summary>
    /// <typeparam name="TRequest">The exact request type.</typeparam>
    /// <typeparam name="TResponse">The exact response type.</typeparam>
    /// <param name="maximumAttempts">The positive total including the first call.</param>
    /// <param name="shouldRetry">
    /// The caller decision after a non-cancellation failure. It receives the
    /// exact request, one-based failed attempt, and original exception. Any
    /// caller-owned wait can occur inside this delegate.
    /// </param>
    /// <returns>Middleware that repeats the same request object when approved.</returns>
    public static AsyncMiddleware<TRequest, TResponse> OnException<TRequest, TResponse>(
        int maximumAttempts,
        Func<TRequest, int, Exception, CancellationToken, ValueTask<bool>> shouldRetry)
    {
        if (maximumAttempts <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumAttempts),
                maximumAttempts,
                "Maximum attempts must be positive.");
        }

        ArgumentNullException.ThrowIfNull(shouldRetry, nameof(shouldRetry));

        return next =>
        {
            ArgumentNullException.ThrowIfNull(next, nameof(next));

            return async (request, cancellationToken) =>
            {
                for (var attempt = 1; ; attempt++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var response = await next(request, cancellationToken)
                            .ConfigureAwait(false);
                        cancellationToken.ThrowIfCancellationRequested();
                        return response;
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception failure)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (attempt >= maximumAttempts
                            || !await shouldRetry(
                                    request,
                                    attempt,
                                    failure,
                                    cancellationToken)
                                .ConfigureAwait(false))
                        {
                            throw;
                        }

                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
            };
        };
    }
}
