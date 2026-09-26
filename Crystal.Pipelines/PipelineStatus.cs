namespace Crystal.Pipelines;

/// <summary>Identifies the observable state of one pipeline invocation.</summary>
public enum PipelineStatus
{
    /// <summary>The invocation has begun.</summary>
    Started,

    /// <summary>The operation or stream completed normally.</summary>
    Succeeded,

    /// <summary>The operation or stream threw an ordinary exception.</summary>
    Failed,

    /// <summary>The operation or stream was canceled.</summary>
    Canceled,

    /// <summary>A stream consumer stopped enumeration before completion.</summary>
    Abandoned
}
