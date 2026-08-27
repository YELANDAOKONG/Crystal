namespace Crystal.Tools;

/// <summary>
/// Contains exact caller-owned text returned by an executable tool or policy.
/// </summary>
public sealed record ToolOutput
{
    /// <summary>
    /// Initializes a successful tool output.
    /// </summary>
    /// <param name="text">The exact caller-owned output text.</param>
    public ToolOutput(string text)
        : this(text, ToolResultStatus.Success)
    {
    }

    /// <summary>
    /// Initializes a tool output.
    /// </summary>
    /// <param name="text">The exact caller-owned output text.</param>
    /// <param name="status">The caller-declared output status.</param>
    public ToolOutput(
        string text,
        ToolResultStatus status)
    {
        ArgumentNullException.ThrowIfNull(text, nameof(text));
        ArgumentNullException.ThrowIfNull(status, nameof(status));

        Text = text;
        Status = status;
    }

    /// <summary>
    /// Gets the exact caller-owned output text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the caller-declared output status.
    /// </summary>
    public ToolResultStatus Status { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(ToolOutput);
}
