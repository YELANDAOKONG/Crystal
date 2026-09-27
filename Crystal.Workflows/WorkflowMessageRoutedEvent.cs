namespace Crystal.Workflows;

/// <summary>Reports one accepted edge without exposing message content.</summary>
public sealed class WorkflowMessageRoutedEvent : WorkflowEvent
{
    internal WorkflowMessageRoutedEvent(
        Guid runId,
        long sequence,
        int superstep,
        string sourceName,
        string targetName,
        int outputIndex)
        : base(runId, sequence, superstep)
    {
        SourceName = sourceName;
        TargetName = targetName;
        OutputIndex = outputIndex;
    }

    /// <summary>Gets the caller-authored source name.</summary>
    public string SourceName { get; }

    /// <summary>Gets the caller-authored target name.</summary>
    public string TargetName { get; }

    /// <summary>Gets the zero-based index within the source output batch.</summary>
    public int OutputIndex { get; }
}
