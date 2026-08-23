using Crystal.Reasoning;

namespace Crystal.Completions;

/// <summary>
/// Carries one readable reasoning-text delta for a completion item.
/// </summary>
public sealed record CompletionReasoningTextDelta : CompletionItemStreamEvent
{
    /// <summary>
    /// Initializes a reasoning-text delta.
    /// </summary>
    /// <param name="candidateIndex">The zero-based candidate index.</param>
    /// <param name="itemIndex">The zero-based item index.</param>
    /// <param name="kind">The readable reasoning classification.</param>
    /// <param name="text">The exact text delta, which may be empty.</param>
    public CompletionReasoningTextDelta(
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
