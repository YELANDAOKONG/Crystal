namespace Crystal.Harness;

/// <summary>
/// Identifies whether the Harness started an Agent invocation.
/// </summary>
public sealed record AgentInvocationOutcome
{
    /// <summary>
    /// Indicates that the Agent ran and returned a result.
    /// </summary>
    public static AgentInvocationOutcome Completed { get; } =
        new("completed");

    /// <summary>
    /// Indicates that the invocation exceeded the shared depth limit.
    /// </summary>
    public static AgentInvocationOutcome DepthLimitReached { get; } =
        new("depth_limit_reached");

    /// <summary>
    /// Indicates that no shared model-call budget remained.
    /// </summary>
    public static AgentInvocationOutcome ModelCallLimitReached { get; } =
        new("model_call_limit_reached");

    /// <summary>
    /// Indicates that no shared wall-clock duration remained.
    /// </summary>
    public static AgentInvocationOutcome DurationLimitReached { get; } =
        new("duration_limit_reached");

    /// <summary>
    /// Initializes an invocation outcome.
    /// </summary>
    /// <param name="value">The outcome value.</param>
    public AgentInvocationOutcome(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));
        Value = value;
    }

    /// <summary>
    /// Gets the outcome value.
    /// </summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
