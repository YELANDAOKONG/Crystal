namespace Crystal.Multimodal.Agents;

/// <summary>Represents one ordered transition in a multimodal Agent run.</summary>
public abstract record MultimodalAgentRunEvent
{
    private protected MultimodalAgentRunEvent(Guid runId, long sequence)
    {
        if (runId == Guid.Empty)
        {
            throw new ArgumentException(
                "Run identifier cannot be empty.",
                nameof(runId));
        }

        if (sequence < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sequence),
                sequence,
                "Event sequence cannot be negative.");
        }

        RunId = runId;
        Sequence = sequence;
    }

    /// <summary>Gets the correlated run identifier.</summary>
    public Guid RunId { get; }

    /// <summary>Gets the zero-based event sequence.</summary>
    public long Sequence { get; }
}
