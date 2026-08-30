namespace Crystal.Multimodal;

/// <summary>Contains one classified readable multimodal reasoning part.</summary>
public sealed record MultimodalReasoningPart
{
    /// <summary>Initializes a readable multimodal reasoning part.</summary>
    /// <param name="content">The exact readable typed content.</param>
    /// <param name="kind">Its provider-originated classification.</param>
    public MultimodalReasoningPart(
        MultimodalContent content,
        MultimodalReasoningKind kind)
    {
        ArgumentNullException.ThrowIfNull(content, nameof(content));
        ArgumentNullException.ThrowIfNull(kind, nameof(kind));

        Content = content;
        Kind = kind;
    }

    /// <summary>Gets the exact readable typed content.</summary>
    public MultimodalContent Content { get; }

    /// <summary>Gets the provider-originated classification.</summary>
    public MultimodalReasoningKind Kind { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(MultimodalReasoningPart);
}
