namespace Crystal.Tools;

/// <summary>
/// Classifies a caller-owned textual tool result.
/// </summary>
public sealed record ToolResultStatus
{
    /// <summary>
    /// Identifies a successful tool result.
    /// </summary>
    public static ToolResultStatus Success { get; } = new("success");

    /// <summary>
    /// Identifies a declared tool failure returned as data.
    /// </summary>
    public static ToolResultStatus Failure { get; } = new("failure");

    /// <summary>
    /// Initializes a tool-result status.
    /// </summary>
    /// <param name="value">The status value.</param>
    public ToolResultStatus(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));
        Value = value;
    }

    /// <summary>
    /// Gets the status value.
    /// </summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
