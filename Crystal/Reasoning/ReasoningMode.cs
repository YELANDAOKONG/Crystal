namespace Crystal.Reasoning;

/// <summary>
/// Describes whether a caller requests model reasoning.
/// </summary>
public sealed record ReasoningMode
{
    /// <summary>
    /// Lets the configured adapter and model use their documented default.
    /// </summary>
    public static ReasoningMode Automatic { get; } = new("automatic");

    /// <summary>
    /// Requests reasoning.
    /// </summary>
    public static ReasoningMode Enabled { get; } = new("enabled");

    /// <summary>
    /// Requests that reasoning be disabled.
    /// </summary>
    public static ReasoningMode Disabled { get; } = new("disabled");

    /// <summary>
    /// Initializes a reasoning mode.
    /// </summary>
    /// <param name="value">The semantic mode value.</param>
    public ReasoningMode(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));
        Value = value;
    }

    /// <summary>
    /// Gets the mode value.
    /// </summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
