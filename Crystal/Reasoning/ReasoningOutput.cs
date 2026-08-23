namespace Crystal.Reasoning;

/// <summary>
/// Describes which readable reasoning surface a caller requests.
/// </summary>
public sealed record ReasoningOutput
{
    /// <summary>
    /// Requests no readable reasoning output.
    /// </summary>
    public static ReasoningOutput None { get; } = new("none");

    /// <summary>
    /// Requests readable reasoning summaries.
    /// </summary>
    public static ReasoningOutput Summary { get; } = new("summary");

    /// <summary>
    /// Requests the fullest readable reasoning surface the adapter supports.
    /// </summary>
    public static ReasoningOutput Full { get; } = new("full");

    /// <summary>
    /// Initializes a reasoning-output request.
    /// </summary>
    /// <param name="value">The semantic output value.</param>
    public ReasoningOutput(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));
        Value = value;
    }

    /// <summary>
    /// Gets the output value.
    /// </summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
