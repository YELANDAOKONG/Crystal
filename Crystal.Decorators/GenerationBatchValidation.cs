using Crystal.Generation.Batches;
using Crystal.Generation.Operations;
using Crystal.Pipelines;

namespace Crystal.Decorators;

/// <summary>Provides opt-in batch result cardinality checks.</summary>
public static class GenerationBatchValidation
{
    /// <summary>
    /// Checks an immediately completed submission against its exact request
    /// count. Pending submissions pass through unchanged.
    /// </summary>
    /// <typeparam name="TRequest">The target-specific request type.</typeparam>
    /// <typeparam name="TResponse">The target-specific response type.</typeparam>
    /// <returns>Middleware for a target-specific StartBatchAsync method.</returns>
    public static AsyncMiddleware<GenerationBatchRequest<TRequest>,
        GenerationOperationSnapshot<GenerationBatchResponse<TResponse>>>
        RequireSubmittedCardinality<TRequest, TResponse>()
        where TRequest : class
        where TResponse : class =>
        next =>
        {
            ArgumentNullException.ThrowIfNull(next);

            return async (request, cancellationToken) =>
            {
                ArgumentNullException.ThrowIfNull(request);
                var snapshot = await next(request, cancellationToken)
                    .ConfigureAwait(false);
                ValidateSnapshot(snapshot, request.Requests.Count);
                return snapshot;
            };
        };

    /// <summary>
    /// Checks a completed polled batch against the caller-retained submitted
    /// request count. Pending snapshots pass through unchanged.
    /// </summary>
    /// <typeparam name="TResponse">The target-specific response type.</typeparam>
    /// <param name="submittedCount">The positive original request count.</param>
    /// <returns>Middleware for a target-specific PollBatchAsync method.</returns>
    public static AsyncMiddleware<GenerationOperationTicket,
        GenerationOperationSnapshot<GenerationBatchResponse<TResponse>>>
        RequirePolledCardinality<TResponse>(int submittedCount)
        where TResponse : class
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(submittedCount);

        return next =>
        {
            ArgumentNullException.ThrowIfNull(next);

            return async (ticket, cancellationToken) =>
            {
                ArgumentNullException.ThrowIfNull(ticket);
                var snapshot = await next(ticket, cancellationToken)
                    .ConfigureAwait(false);
                ValidateSnapshot(snapshot, submittedCount);
                return snapshot;
            };
        };
    }

    private static void ValidateSnapshot<TResponse>(
        GenerationOperationSnapshot<GenerationBatchResponse<TResponse>>? snapshot,
        int submittedCount)
        where TResponse : class
    {
        if (snapshot is null)
        {
            throw new InvalidOperationException(
                "The generation batch client returned no operation snapshot.");
        }

        if (snapshot.Response is { } response
            && response.InputCount != submittedCount)
        {
            throw new InvalidOperationException(
                "The generation batch response has an unexpected item count.");
        }
    }
}
