namespace Crystal.Workflows;

/// <summary>Identifies one ordered workflow transition without payload data.</summary>
public abstract class WorkflowEvent
{
    private protected WorkflowEvent(
        Guid runId,
        long sequence,
        int superstep)
    {
        RunId = runId;
        Sequence = sequence;
        Superstep = superstep;
    }

    /// <summary>Gets the caller-supplied run identifier.</summary>
    public Guid RunId { get; }

    /// <summary>Gets the zero-based event sequence.</summary>
    public long Sequence { get; }

    /// <summary>Gets the one-based superstep, or zero for pre-execution stop.</summary>
    public int Superstep { get; }

    /// <inheritdoc />
    public sealed override string ToString() => GetType().Name;
}
