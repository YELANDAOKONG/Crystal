namespace Crystal.Agents;

/// <summary>
/// Marks an Agent stream as complete and carries its final result.
/// </summary>
public sealed record AgentRunCompletedEvent : AgentRunEvent
{
    /// <summary>
    /// Initializes an Agent completion event.
    /// </summary>
    /// <param name="runId">The run identifier.</param>
    /// <param name="sequence">The zero-based event sequence.</param>
    /// <param name="result">The final run result.</param>
    public AgentRunCompletedEvent(
        Guid runId,
        long sequence,
        AgentRunResult result)
        : base(runId, sequence)
    {
        ArgumentNullException.ThrowIfNull(result, nameof(result));

        if (result.RunId != runId)
        {
            throw new ArgumentException(
                "The result run identifier does not match the event.",
                nameof(result));
        }

        Result = result;
    }

    /// <summary>
    /// Gets the final run result.
    /// </summary>
    public AgentRunResult Result { get; }
}
