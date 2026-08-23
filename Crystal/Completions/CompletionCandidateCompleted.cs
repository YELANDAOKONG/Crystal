namespace Crystal.Completions;

/// <summary>
/// Marks one completion candidate as complete.
/// </summary>
public sealed record CompletionCandidateCompleted : CompletionCandidateStreamEvent
{
    /// <summary>
    /// Initializes a candidate-completed event.
    /// </summary>
    /// <param name="candidateIndex">The zero-based candidate index.</param>
    /// <param name="finishReason">The provider-reported finish reason.</param>
    public CompletionCandidateCompleted(
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
