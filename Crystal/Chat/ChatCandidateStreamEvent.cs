namespace Crystal.Chat;

/// <summary>
/// Represents a chat-stream event for one candidate.
/// </summary>
public abstract record ChatCandidateStreamEvent : ChatStreamEvent
{
    private protected ChatCandidateStreamEvent(int candidateIndex)
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
