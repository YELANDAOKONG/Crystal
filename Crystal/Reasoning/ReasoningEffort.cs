namespace Crystal.Reasoning;

/// <summary>
/// Describes the requested relative reasoning effort.
/// </summary>
public sealed record ReasoningEffort
{
    /// <summary>
    /// Gets the conventional minimal effort.
    /// </summary>
    public static ReasoningEffort Minimal { get; } = new("minimal");

    /// <summary>
    /// Gets the conventional low effort.
    /// </summary>
    public static ReasoningEffort Low { get; } = new("low");

    /// <summary>
    /// Gets the conventional medium effort.
    /// </summary>
    public static ReasoningEffort Medium { get; } = new("medium");

    /// <summary>
    /// Gets the conventional high effort.
    /// </summary>
    public static ReasoningEffort High { get; } = new("high");

    /// <summary>
    /// Gets the conventional maximum effort.
    /// </summary>
    public static ReasoningEffort Maximum { get; } = new("maximum");

    /// <summary>
    /// Initializes a reasoning effort.
    /// </summary>
    /// <param name="value">The semantic effort value.</param>
    public ReasoningEffort(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));
        Value = value;
    }

    /// <summary>
    /// Gets the effort value.
    /// </summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
