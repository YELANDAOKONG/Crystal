using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Crystal.Pipelines;

/// <summary>Creates opt-in, content-free observation middleware.</summary>
public static class PipelineDiagnostics
{
    /// <summary>Observes a complete asynchronous operation.</summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="operationName">A stable caller-supplied name without sensitive content.</param>
    /// <param name="observer">The caller-owned observer. Its exceptions propagate.</param>
    /// <returns>Middleware that reports start, outcome, and elapsed time.</returns>
    public static AsyncMiddleware<TRequest, TResponse> ObserveAsync<TRequest, TResponse>(
        string operationName,
        Action<PipelineObservation> observer)
    {
        Validate(operationName, observer);

        return next => async (request, cancellationToken) =>
        {
            var started = Stopwatch.GetTimestamp();
            observer(new PipelineObservation(operationName, PipelineStatus.Started, TimeSpan.Zero));
            var status = PipelineStatus.Succeeded;

            try
            {
                return await next(request, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                status = PipelineStatus.Canceled;
                throw;
            }
            catch
            {
                status = PipelineStatus.Failed;
                throw;
            }
            finally
            {
                observer(new PipelineObservation(
                    operationName,
                    status,
                    Stopwatch.GetElapsedTime(started)));
            }
        };
    }

    /// <summary>Observes an asynchronous stream from enumeration to disposal.</summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TEvent">The streamed event type.</typeparam>
    /// <param name="operationName">A stable caller-supplied name without sensitive content.</param>
    /// <param name="observer">The caller-owned observer. Its exceptions propagate.</param>
    /// <returns>Middleware that reports start, outcome, and elapsed time.</returns>
    public static StreamingMiddleware<TRequest, TEvent> ObserveStreaming<TRequest, TEvent>(
        string operationName,
        Action<PipelineObservation> observer)
    {
        Validate(operationName, observer);

        return next => (request, cancellationToken) =>
            ObserveStream(next, request, cancellationToken, operationName, observer);
    }

    private static async IAsyncEnumerable<TEvent> ObserveStream<TRequest, TEvent>(
        StreamingOperation<TRequest, TEvent> next,
        TRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken,
        string operationName,
        Action<PipelineObservation> observer)
    {
        var started = Stopwatch.GetTimestamp();
        observer(new PipelineObservation(operationName, PipelineStatus.Started, TimeSpan.Zero));
        var status = PipelineStatus.Abandoned;
        IAsyncEnumerator<TEvent>? enumerator = null;

        try
        {
            try
            {
                enumerator = next(request, cancellationToken)
                    .GetAsyncEnumerator(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                status = PipelineStatus.Canceled;
                throw;
            }
            catch
            {
                status = PipelineStatus.Failed;
                throw;
            }

            while (true)
            {
                bool hasNext;
                try
                {
                    hasNext = await enumerator.MoveNextAsync().ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    status = PipelineStatus.Canceled;
                    throw;
                }
                catch
                {
                    status = PipelineStatus.Failed;
                    throw;
                }

                if (!hasNext)
                {
                    status = PipelineStatus.Succeeded;
                    break;
                }

                TEvent current;
                try
                {
                    current = enumerator.Current;
                }
                catch (OperationCanceledException)
                {
                    status = PipelineStatus.Canceled;
                    throw;
                }
                catch
                {
                    status = PipelineStatus.Failed;
                    throw;
                }

                yield return current;
            }
        }
        finally
        {
            try
            {
                if (enumerator is not null)
                {
                    await enumerator.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                status = PipelineStatus.Canceled;
                throw;
            }
            catch
            {
                status = PipelineStatus.Failed;
                throw;
            }
            finally
            {
                if (status == PipelineStatus.Abandoned
                    && cancellationToken.IsCancellationRequested)
                {
                    status = PipelineStatus.Canceled;
                }

                observer(new PipelineObservation(
                    operationName,
                    status,
                    Stopwatch.GetElapsedTime(started)));
            }
        }
    }

    private static void Validate(
        string operationName,
        Action<PipelineObservation> observer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName, nameof(operationName));
        ArgumentNullException.ThrowIfNull(observer, nameof(observer));
    }
}
