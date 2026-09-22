namespace Crystal.Multimodal.Chat;

/// <summary>Represents a multimodal Chat stream event for one candidate.</summary>
public abstract record MultimodalChatCandidateStreamEvent
    : MultimodalChatStreamEvent
{
    private protected MultimodalChatCandidateStreamEvent(int candidateIndex)
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

    /// <summary>Gets the zero-based candidate index.</summary>
    public int CandidateIndex { get; }
}
