namespace Crystal.Multimodal.Chat;

/// <summary>Contains one ordered readable or opaque reasoning block.</summary>
public sealed record MultimodalReasoningItem : MultimodalChatItem
{
    /// <summary>Initializes a multimodal reasoning item.</summary>
    /// <param name="content">The exact reasoning content.</param>
    public MultimodalReasoningItem(MultimodalReasoningContent content)
    {
        ArgumentNullException.ThrowIfNull(content, nameof(content));
        Content = content;
    }

    /// <summary>Gets the exact reasoning content.</summary>
    public MultimodalReasoningContent Content { get; }
}
