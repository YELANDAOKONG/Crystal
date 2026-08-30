namespace Crystal.Multimodal.Agents;

/// <summary>Marks a multimodal Agent stream complete with its result.</summary>
public sealed record MultimodalAgentRunCompletedEvent
    : MultimodalAgentRunEvent
{
    /// <summary>Initializes a multimodal Agent completion event.</summary>
    /// <param name="runId">The run identifier.</param>
    /// <param name="sequence">The zero-based event sequence.</param>
    /// <param name="result">The final run result.</param>
    public MultimodalAgentRunCompletedEvent(
        Guid runId,
        long sequence,
        MultimodalAgentRunResult result)
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

    /// <summary>Gets the final run result.</summary>
    public MultimodalAgentRunResult Result { get; }
}
