using Crystal.Reasoning;

namespace Crystal.Chat;

/// <summary>
/// Contains one provider-native reasoning block in a chat transcript.
/// </summary>
public sealed record ChatReasoningItem : ChatItem
{
    /// <summary>
    /// Initializes a chat reasoning item.
    /// </summary>
    /// <param name="content">The preserved reasoning block.</param>
    public ChatReasoningItem(ReasoningContent content)
    {
        ArgumentNullException.ThrowIfNull(content, nameof(content));
        Content = content;
    }

    /// <summary>
    /// Gets the preserved reasoning block.
    /// </summary>
    public ReasoningContent Content { get; }
}
