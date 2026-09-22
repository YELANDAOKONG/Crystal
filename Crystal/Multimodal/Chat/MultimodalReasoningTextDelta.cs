namespace Crystal.Multimodal.Chat;

/// <summary>Carries one exact text delta for a reasoning part.</summary>
public sealed record MultimodalReasoningTextDelta
    : MultimodalChatItemStreamEvent
{
    /// <summary>Initializes a reasoning text delta.</summary>
    /// <param name="candidateIndex">The zero-based candidate index.</param>
    /// <param name="itemIndex">The zero-based item index.</param>
    /// <param name="partIndex">
    /// The zero-based readable-part index within the reasoning item.
    /// </param>
    /// <param name="kind">The readable reasoning classification.</param>
    /// <param name="text">The exact text delta, which may be empty.</param>
    public MultimodalReasoningTextDelta(
        int candidateIndex,
        int itemIndex,
        int partIndex,
        MultimodalReasoningKind kind,
        string text)
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
        ArgumentNullException.ThrowIfNull(text, nameof(text));

        PartIndex = partIndex;
        Kind = kind;
        Text = text;
    }

    /// <summary>Gets the zero-based readable-part index.</summary>
    public int PartIndex { get; }

    /// <summary>Gets the readable reasoning classification.</summary>
    public MultimodalReasoningKind Kind { get; }

    /// <summary>Gets the exact text delta.</summary>
    public string Text { get; }
}
