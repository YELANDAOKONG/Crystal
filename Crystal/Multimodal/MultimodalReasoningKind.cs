namespace Crystal.Multimodal;

/// <summary>Classifies readable multimodal reasoning without changing it.</summary>
public sealed record MultimodalReasoningKind
{
    /// <summary>Identifies a model-produced reasoning summary.</summary>
    public static MultimodalReasoningKind Summary { get; } = new("summary");

    /// <summary>Identifies a model-produced reasoning trace.</summary>
    public static MultimodalReasoningKind Trace { get; } = new("trace");

    /// <summary>Initializes a multimodal reasoning classification.</summary>
    /// <param name="value">The complete classification value.</param>
    public MultimodalReasoningKind(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));
        Value = value;
    }

    /// <summary>Gets the complete classification value.</summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
