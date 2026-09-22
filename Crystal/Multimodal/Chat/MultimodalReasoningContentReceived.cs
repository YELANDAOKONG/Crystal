namespace Crystal.Multimodal.Chat;

/// <summary>Carries one complete typed content block for a reasoning part.</summary>
public sealed record MultimodalReasoningContentReceived
    : MultimodalChatItemStreamEvent
{
    /// <summary>Initializes a complete reasoning-content event.</summary>
    /// <param name="candidateIndex">The zero-based candidate index.</param>
    /// <param name="itemIndex">The zero-based item index.</param>
    /// <param name="partIndex">
    /// The zero-based readable-part index within the reasoning item.
    /// </param>
    /// <param name="kind">The readable reasoning classification.</param>
    /// <param name="content">The complete exact typed content block.</param>
    public MultimodalReasoningContentReceived(
        int candidateIndex,
        int itemIndex,
        int partIndex,
        MultimodalReasoningKind kind,
        MultimodalContent content)
        : base(candidateIndex, itemIndex)
    {
        if (partIndex < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(partIndex),
                partIndex,
                "Reasoning-part index cannot be negative.");
        }

        ArgumentNullException.ThrowIfNull(kind, nameof(kind));
        ArgumentNullException.ThrowIfNull(content, nameof(content));

        PartIndex = partIndex;
        Kind = kind;
        Content = content;
    }

    /// <summary>Gets the zero-based readable-part index.</summary>
    public int PartIndex { get; }

    /// <summary>Gets the readable reasoning classification.</summary>
    public MultimodalReasoningKind Kind { get; }

    /// <summary>Gets the complete exact typed content block.</summary>
    public MultimodalContent Content { get; }
}
