namespace Crystal.Workflows;

/// <summary>Reports one completed node invocation using metadata only.</summary>
public sealed class WorkflowNodeCompletedEvent : WorkflowEvent
{
    internal WorkflowNodeCompletedEvent(
        Guid runId,
        long sequence,
        int superstep,
        string nodeName,
        int inputCount,
        int outputCount)
        : base(runId, sequence, superstep)
    {
        NodeName = nodeName;
        InputCount = inputCount;
        OutputCount = outputCount;
    }

    /// <summary>Gets the caller-authored node name.</summary>
    public string NodeName { get; }

    /// <summary>Gets the received message count.</summary>
    public int InputCount { get; }

    /// <summary>Gets the emitted message count.</summary>
    public int OutputCount { get; }
}
