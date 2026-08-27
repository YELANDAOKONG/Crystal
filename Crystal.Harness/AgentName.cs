namespace Crystal.Harness;

/// <summary>
/// Identifies one registered Agent within a Harness.
/// </summary>
public sealed record AgentName
{
    /// <summary>
    /// Initializes an Agent name.
    /// </summary>
    /// <param name="value">The stable case-sensitive name.</param>
    public AgentName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));
        Value = value;
    }

    /// <summary>
    /// Gets the stable case-sensitive name.
    /// </summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
