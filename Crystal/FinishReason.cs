namespace Crystal;

/// <summary>
/// Identifies why a provider stopped producing one model candidate.
/// </summary>
public sealed record FinishReason
{
    /// <summary>
    /// Gets the conventional value for normal completion.
    /// </summary>
    public static FinishReason Stop { get; } = new("stop");

    /// <summary>
    /// Gets the conventional value for an output-length limit.
    /// </summary>
    public static FinishReason Length { get; } = new("length");

    /// <summary>
    /// Gets the conventional value for a candidate that requests tools.
    /// </summary>
    public static FinishReason ToolCalls { get; } = new("tool_calls");

    /// <summary>
    /// Gets the conventional value for provider content filtering.
    /// </summary>
    public static FinishReason ContentFilter { get; } = new("content_filter");

    /// <summary>
    /// Initializes a new finish reason.
    /// </summary>
    /// <param name="value">The provider-neutral or provider-originated value.</param>
    public FinishReason(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));
        Value = value;
    }

    /// <summary>
    /// Gets the unmodified finish-reason value.
    /// </summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
