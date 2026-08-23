namespace Crystal.Chat;

/// <summary>
/// Marks one text-chat candidate as complete.
/// </summary>
public sealed record ChatCandidateCompleted : ChatCandidateStreamEvent
{
    /// <summary>
    /// Initializes a candidate-completed event.
    /// </summary>
    /// <param name="candidateIndex">The zero-based candidate index.</param>
    /// <param name="finishReason">The provider-reported finish reason.</param>
    public ChatCandidateCompleted(
        int candidateIndex,
        FinishReason finishReason)
        : base(candidateIndex)
    {
        ArgumentNullException.ThrowIfNull(finishReason, nameof(finishReason));
        FinishReason = finishReason;
    }

    /// <summary>
    /// Gets the finish reason.
    /// </summary>
    public FinishReason FinishReason { get; }
}
