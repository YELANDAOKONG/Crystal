namespace Crystal.Multimodal.Chat;

/// <summary>Carries one complete typed content block for a tool call.</summary>
public sealed record MultimodalToolCallContentReceived
    : MultimodalChatItemStreamEvent
{
    /// <summary>Initializes a complete tool-call-content event.</summary>
    /// <param name="candidateIndex">The zero-based candidate index.</param>
    /// <param name="itemIndex">The zero-based item index.</param>
    /// <param name="contentIndex">
    /// The zero-based content-block index within the tool call.
    /// </param>
    /// <param name="content">The complete exact typed content block.</param>
    public MultimodalToolCallContentReceived(
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
