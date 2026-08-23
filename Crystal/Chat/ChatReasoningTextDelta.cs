using Crystal.Reasoning;

namespace Crystal.Chat;

/// <summary>
/// Carries one readable reasoning-text delta for a chat item.
/// </summary>
public sealed record ChatReasoningTextDelta : ChatItemStreamEvent
{
    /// <summary>
    /// Initializes a reasoning-text delta.
    /// </summary>
    /// <param name="candidateIndex">The zero-based candidate index.</param>
    /// <param name="itemIndex">The zero-based item index.</param>
    /// <param name="textSegmentIndex">
    /// The zero-based text-segment index within the reasoning item.
    /// </param>
    /// <param name="kind">The readable reasoning classification.</param>
    /// <param name="text">The exact text delta, which may be empty.</param>
    public ChatReasoningTextDelta(
        int candidateIndex,
        int itemIndex,
        int textSegmentIndex,
        ReasoningTextKind kind,
        string text)
        : base(candidateIndex, itemIndex)
    {
        if (textSegmentIndex < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(textSegmentIndex),
                textSegmentIndex,
                "Text-segment index cannot be negative.");
        }

        ArgumentNullException.ThrowIfNull(kind, nameof(kind));
        ArgumentNullException.ThrowIfNull(text, nameof(text));

        TextSegmentIndex = textSegmentIndex;
        Kind = kind;
        Text = text;
    }

    /// <summary>
    /// Gets the zero-based text-segment index within the reasoning item.
    /// </summary>
    public int TextSegmentIndex { get; }

    /// <summary>
    /// Gets the reasoning-text classification.
    /// </summary>
    public ReasoningTextKind Kind { get; }

    /// <summary>
    /// Gets the exact text delta.
    /// </summary>
    public string Text { get; }
}
