namespace Crystal.Completions;

/// <summary>
/// Represents a completion-stream event for one candidate.
/// </summary>
public abstract record CompletionCandidateStreamEvent : CompletionStreamEvent
{
    private protected CompletionCandidateStreamEvent(int candidateIndex)
    {
        if (candidateIndex < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(candidateIndex),
                candidateIndex,
                "Candidate index cannot be negative.");
        }

        CandidateIndex = candidateIndex;
    }

    /// <summary>
    /// Gets the zero-based candidate index.
    /// </summary>
    public int CandidateIndex { get; }
}
