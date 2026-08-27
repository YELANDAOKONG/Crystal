using Crystal.Chat;

namespace Crystal.Agents;

/// <summary>
/// Records the exact candidate selected by caller-owned policy.
/// </summary>
public sealed record AgentCandidateSelectedEvent : AgentRunEvent
{
    /// <summary>
    /// Initializes a candidate-selection event.
    /// </summary>
    /// <param name="runId">The run identifier.</param>
    /// <param name="sequence">The zero-based event sequence.</param>
    /// <param name="modelCallNumber">The one-based model-call number.</param>
    /// <param name="candidateIndex">The zero-based candidate index.</param>
    /// <param name="candidate">The exact selected candidate.</param>
    public AgentCandidateSelectedEvent(
        Guid runId,
        long sequence,
        int modelCallNumber,
        int candidateIndex,
        ChatCandidate candidate)
        : base(runId, sequence)
    {
        if (modelCallNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(modelCallNumber),
                modelCallNumber,
                "Model call number must be positive.");
        }

        if (candidateIndex < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(candidateIndex),
                candidateIndex,
                "Candidate index cannot be negative.");
        }

        ArgumentNullException.ThrowIfNull(candidate, nameof(candidate));

        ModelCallNumber = modelCallNumber;
        CandidateIndex = candidateIndex;
        Candidate = candidate;
    }

    /// <summary>
    /// Gets the one-based model-call number.
    /// </summary>
    public int ModelCallNumber { get; }

    /// <summary>
    /// Gets the zero-based selected candidate index.
    /// </summary>
    public int CandidateIndex { get; }

    /// <summary>
    /// Gets the exact selected candidate.
    /// </summary>
    public ChatCandidate Candidate { get; }
}
