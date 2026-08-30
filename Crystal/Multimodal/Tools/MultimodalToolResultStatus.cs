namespace Crystal.Multimodal.Tools;

/// <summary>Classifies a caller-owned multimodal tool result.</summary>
public sealed record MultimodalToolResultStatus
{
    /// <summary>Identifies a successful tool result.</summary>
    public static MultimodalToolResultStatus Success { get; } =
        new("success");

    /// <summary>Identifies a declared tool failure returned as data.</summary>
    public static MultimodalToolResultStatus Failure { get; } =
        new("failure");

    /// <summary>Initializes a multimodal tool-result status.</summary>
    /// <param name="value">The complete status value.</param>
    public MultimodalToolResultStatus(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));
        Value = value;
    }

    /// <summary>Gets the complete status value.</summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
