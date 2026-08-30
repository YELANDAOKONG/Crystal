namespace Crystal.Multimodal.Agents;

/// <summary>Identifies why a multimodal Agent run stopped.</summary>
public sealed record MultimodalAgentRunStopReason
{
    /// <summary>Indicates that the selected candidate was terminal.</summary>
    public static MultimodalAgentRunStopReason Completed { get; } =
        new("completed");

    /// <summary>Indicates that another model call would exceed its limit.</summary>
    public static MultimodalAgentRunStopReason ModelCallLimitReached { get; } =
        new("model_call_limit_reached");

    /// <summary>Indicates that a tool batch would exceed its limit.</summary>
    public static MultimodalAgentRunStopReason ToolCallLimitReached { get; } =
        new("tool_call_limit_reached");

    /// <summary>Indicates that the configured wall-clock duration elapsed.</summary>
    public static MultimodalAgentRunStopReason DurationLimitReached { get; } =
        new("duration_limit_reached");

    /// <summary>Initializes a multimodal Agent stop reason.</summary>
    /// <param name="value">The complete stop-reason value.</param>
    public MultimodalAgentRunStopReason(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));
        Value = value;
    }

    /// <summary>Gets the complete stop-reason value.</summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
