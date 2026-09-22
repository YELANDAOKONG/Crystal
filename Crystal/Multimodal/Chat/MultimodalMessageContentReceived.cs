namespace Crystal.Multimodal.Chat;

/// <summary>Carries one complete typed content block for a message.</summary>
public sealed record MultimodalMessageContentReceived
    : MultimodalChatItemStreamEvent
{
    /// <summary>Initializes a complete message-content event.</summary>
    /// <param name="candidateIndex">The zero-based candidate index.</param>
    /// <param name="itemIndex">The zero-based item index.</param>
    /// <param name="contentIndex">
    /// The zero-based content-block index within the message.
    /// </param>
    /// <param name="content">The complete exact typed content block.</param>
    public MultimodalMessageContentReceived(
        int candidateIndex,
        int itemIndex,
        int contentIndex,
        MultimodalContent content)
        : base(candidateIndex, itemIndex)
    {
        if (contentIndex < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(contentIndex),
                contentIndex,
                "Content index cannot be negative.");
        }

        ArgumentNullException.ThrowIfNull(content, nameof(content));
        ContentIndex = contentIndex;
        Content = content;
    }

    /// <summary>Gets the zero-based content-block index.</summary>
    public int ContentIndex { get; }

    /// <summary>Gets the complete exact typed content block.</summary>
    public MultimodalContent Content { get; }
}
