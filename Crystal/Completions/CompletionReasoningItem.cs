using Crystal.Reasoning;

namespace Crystal.Completions;

/// <summary>
/// Contains one provider-native reasoning block in a completion candidate.
/// </summary>
public sealed record CompletionReasoningItem : CompletionItem
{
    /// <summary>
    /// Initializes a completion reasoning item.
    /// </summary>
    /// <param name="content">The preserved reasoning block.</param>
    public CompletionReasoningItem(ReasoningContent content)
    {
        ArgumentNullException.ThrowIfNull(content, nameof(content));
        Content = content;
    }

    /// <summary>
    /// Gets the preserved reasoning block.
    /// </summary>
    public ReasoningContent Content { get; }
}
