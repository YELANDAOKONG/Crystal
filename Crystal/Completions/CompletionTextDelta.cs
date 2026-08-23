namespace Crystal.Completions;

/// <summary>
/// Carries one exact text delta for a completion item.
/// </summary>
public sealed record CompletionTextDelta : CompletionItemStreamEvent
{
    /// <summary>
    /// Initializes a text delta.
    /// </summary>
    /// <param name="candidateIndex">The zero-based candidate index.</param>
    /// <param name="itemIndex">The zero-based item index.</param>
    /// <param name="text">The exact text delta, which may be empty.</param>
    public CompletionTextDelta(
        int candidateIndex,
        int itemIndex,
        string text)
        : base(candidateIndex, itemIndex)
    {
        ArgumentNullException.ThrowIfNull(text, nameof(text));
        Text = text;
    }

    /// <summary>
    /// Gets the exact text delta.
    /// </summary>
    public string Text { get; }
}
