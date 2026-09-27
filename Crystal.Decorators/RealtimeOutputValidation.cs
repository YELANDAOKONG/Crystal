using System.Runtime.CompilerServices;

using Crystal.Realtime;

namespace Crystal.Decorators;

/// <summary>Provides opt-in ordering checks for live-session output events.</summary>
public static class RealtimeOutputValidation
{
    /// <summary>
    /// Forwards exact output events while rejecting noncontiguous content
    /// segments, duplicate completion, and events for a completed output.
    /// </summary>
    /// <param name="source">The live session's output stream.</param>
    /// <param name="cancellationToken">Cancels enumeration and local waiting.</param>
    /// <returns>The same accepted event objects in their original order.</returns>
    public static async IAsyncEnumerable<RealtimeOutputEvent> ValidateAsync(
        IAsyncEnumerable<RealtimeOutputEvent> source,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        cancellationToken.ThrowIfCancellationRequested();

        var nextSegmentIndexes = new Dictionary<string, int>(StringComparer.Ordinal);
        var completed = new HashSet<string>(StringComparer.Ordinal);

        await foreach (var outputEvent in source
            .WithCancellation(cancellationToken)
            .ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (outputEvent)
            {
                case RealtimeOutputContentReceived content:
                    if (completed.Contains(content.OutputId)
                        || content.SegmentIndex != nextSegmentIndexes.GetValueOrDefault(
                            content.OutputId))
                    {
                        throw new InvalidOperationException(
                            "The realtime output contains an invalid content sequence.");
                    }

                    nextSegmentIndexes[content.OutputId] =
                        checked(content.SegmentIndex + 1);
                    break;

                case RealtimeOutputReasoningReceived reasoning:
                    if (completed.Contains(reasoning.OutputId))
                    {
                        throw new InvalidOperationException(
                            "The realtime output continued after its completion.");
                    }

                    break;

                case RealtimeOutputTurnCompleted terminal:
                    if (!completed.Add(terminal.OutputId))
                    {
                        throw new InvalidOperationException(
                            "The realtime output completed more than once.");
                    }

                    break;

                case RealtimeOutputToolCallReceived:
                    break;

                default:
                    throw new InvalidOperationException(
                        "The realtime session returned an unknown output event.");
            }

            yield return outputEvent;
        }

        cancellationToken.ThrowIfCancellationRequested();
    }
}
