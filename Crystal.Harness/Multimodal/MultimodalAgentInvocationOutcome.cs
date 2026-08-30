namespace Crystal.Multimodal.Harness;

/// <summary>
/// Identifies whether the Harness started a multimodal Agent invocation.
/// </summary>
public sealed record MultimodalAgentInvocationOutcome
{
    /// <summary>Indicates that the Agent ran and returned a result.</summary>
    public static MultimodalAgentInvocationOutcome Completed { get; } =
        new("completed");

    /// <summary>Indicates that the invocation exceeded shared depth.</summary>
    public static MultimodalAgentInvocationOutcome DepthLimitReached { get; } =
        new("depth_limit_reached");

    /// <summary>Indicates that no shared model-call budget remained.</summary>
    public static MultimodalAgentInvocationOutcome
        ModelCallLimitReached { get; } =
            new("model_call_limit_reached");

    /// <summary>Indicates that no shared wall-clock duration remained.</summary>
    public static MultimodalAgentInvocationOutcome DurationLimitReached { get; } =
        new("duration_limit_reached");

    /// <summary>Initializes a multimodal invocation outcome.</summary>
    /// <param name="value">The complete outcome value.</param>
    public MultimodalAgentInvocationOutcome(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));
        Value = value;
    }

    /// <summary>Gets the complete outcome value.</summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
