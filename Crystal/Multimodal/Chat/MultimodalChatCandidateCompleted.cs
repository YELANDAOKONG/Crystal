namespace Crystal.Multimodal.Chat;

/// <summary>Marks one multimodal Chat candidate as complete.</summary>
public sealed record MultimodalChatCandidateCompleted
    : MultimodalChatCandidateStreamEvent
{
    /// <summary>Initializes a candidate-completed event.</summary>
    /// <param name="candidateIndex">The zero-based candidate index.</param>
    /// <param name="finishReason">The provider-reported finish reason.</param>
    public MultimodalChatCandidateCompleted(
        int candidateIndex,
        FinishReason finishReason)
        : base(candidateIndex)
    {
        ArgumentNullException.ThrowIfNull(finishReason, nameof(finishReason));
        FinishReason = finishReason;
    }

    /// <summary>Gets the finish reason.</summary>
    public FinishReason FinishReason { get; }
}
