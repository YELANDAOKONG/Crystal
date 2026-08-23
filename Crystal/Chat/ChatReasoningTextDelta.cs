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
    /// <param name="kind">The readable reasoning classification.</param>
    /// <param name="text">The exact text delta, which may be empty.</param>
    public ChatReasoningTextDelta(
        int candidateIndex,
        int itemIndex,
        ReasoningTextKind kind,
        string text)
        : base(candidateIndex, itemIndex)
    {
        ArgumentNullException.ThrowIfNull(kind, nameof(kind));
        ArgumentNullException.ThrowIfNull(text, nameof(text));

        Kind = kind;
        Text = text;
    }

    /// <summary>
    /// Gets the reasoning-text classification.
    /// </summary>
    public ReasoningTextKind Kind { get; }

    /// <summary>
    /// Gets the exact text delta.
    /// </summary>
    public string Text { get; }
}
