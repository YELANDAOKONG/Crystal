namespace Crystal.Multimodal.Chat;

/// <summary>Carries one exact text delta for a multimodal message block.</summary>
public sealed record MultimodalMessageTextDelta
    : MultimodalChatItemStreamEvent
{
    /// <summary>Initializes a message text delta.</summary>
    /// <param name="candidateIndex">The zero-based candidate index.</param>
    /// <param name="itemIndex">The zero-based item index.</param>
    /// <param name="contentIndex">
    /// The zero-based content-block index within the message.
    /// </param>
    /// <param name="text">The exact text delta, which may be empty.</param>
    public MultimodalMessageTextDelta(
        int candidateIndex,
        int itemIndex,
        int contentIndex,
        string text)
        : base(candidateIndex, itemIndex)
    {
        if (contentIndex < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(contentIndex),
                contentIndex,
                "Content index cannot be negative.");
        }

        ArgumentNullException.ThrowIfNull(text, nameof(text));
        ContentIndex = contentIndex;
        Text = text;
    }

    /// <summary>Gets the zero-based content-block index.</summary>
    public int ContentIndex { get; }

    /// <summary>Gets the exact text delta.</summary>
    public string Text { get; }
}
