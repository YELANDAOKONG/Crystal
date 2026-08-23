namespace Crystal.Agents;

/// <summary>
/// Identifies why an Agent run stopped.
/// </summary>
public sealed record AgentRunStopReason
{
    /// <summary>
    /// Indicates that the selected model candidate was terminal.
    /// </summary>
    public static AgentRunStopReason Completed { get; } = new("completed");

    /// <summary>
    /// Indicates that another model call would exceed the configured limit.
    /// </summary>
    public static AgentRunStopReason ModelCallLimitReached { get; } =
        new("model_call_limit_reached");

    /// <summary>
    /// Indicates that a requested tool batch would exceed the configured limit.
    /// </summary>
    public static AgentRunStopReason ToolCallLimitReached { get; } =
        new("tool_call_limit_reached");

    /// <summary>
    /// Indicates that the configured wall-clock duration elapsed.
    /// </summary>
    public static AgentRunStopReason DurationLimitReached { get; } =
        new("duration_limit_reached");

    /// <summary>
    /// Initializes an Agent stop reason.
    /// </summary>
    /// <param name="value">The stop-reason value.</param>
    public AgentRunStopReason(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));
        Value = value;
    }

    /// <summary>
    /// Gets the stop-reason value.
    /// </summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
