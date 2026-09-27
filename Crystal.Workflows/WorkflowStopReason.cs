namespace Crystal.Workflows;

/// <summary>Identifies why one workflow invocation stopped.</summary>
public sealed record WorkflowStopReason
{
    /// <summary>All routed work finished.</summary>
    public static WorkflowStopReason Completed { get; } = new("completed");

    /// <summary>Another superstep would exceed the configured limit.</summary>
    public static WorkflowStopReason SuperstepLimitReached { get; } =
        new("superstep_limit_reached");

    private WorkflowStopReason(string value)
    {
        Value = value;
    }

    /// <summary>Gets the stable reason value.</summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
