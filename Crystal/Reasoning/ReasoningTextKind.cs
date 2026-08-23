namespace Crystal.Reasoning;

/// <summary>
/// Classifies readable reasoning text without changing it.
/// </summary>
public sealed record ReasoningTextKind
{
    /// <summary>
    /// Identifies a model-produced reasoning summary.
    /// </summary>
    public static ReasoningTextKind Summary { get; } = new("summary");

    /// <summary>
    /// Identifies a model-produced reasoning trace.
    /// </summary>
    public static ReasoningTextKind Trace { get; } = new("trace");

    /// <summary>
    /// Initializes a reasoning-text classification.
    /// </summary>
    /// <param name="value">The classification value.</param>
    public ReasoningTextKind(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));
        Value = value;
    }

    /// <summary>
    /// Gets the classification value.
    /// </summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
