using Crystal.Internal;

namespace Crystal.Generation;

internal static class GenerationRequestRules
{
    public static IReadOnlyList<GenerationInput> SnapshotInputs(
        IEnumerable<GenerationInput> inputs) =>
        CollectionSnapshot.Create(inputs, nameof(inputs));

    public static void ValidateRequestedCandidateCount(int? requestedCandidateCount)
    {
        if (requestedCandidateCount is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedCandidateCount),
                requestedCandidateCount,
                "Requested candidate count must be positive.");
        }
    }
}
