namespace Crystal.Multimodal.Harness;

/// <summary>Identifies one multimodal Agent registered in a Harness.</summary>
public sealed record MultimodalAgentName
{
    /// <summary>Initializes a multimodal Agent name.</summary>
    /// <param name="value">The stable case-sensitive name.</param>
    public MultimodalAgentName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));
        Value = value;
    }

    /// <summary>Gets the stable case-sensitive name.</summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
