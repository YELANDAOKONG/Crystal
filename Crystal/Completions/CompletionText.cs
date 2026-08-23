namespace Crystal.Completions;

/// <summary>
/// Contains one exact text segment in a completion candidate.
/// </summary>
public sealed record CompletionText : CompletionItem
{
    /// <summary>
    /// Initializes a completion text segment.
    /// </summary>
    /// <param name="text">The exact provider-produced text.</param>
    public CompletionText(string text)
    {
        ArgumentNullException.ThrowIfNull(text, nameof(text));
        Text = text;
    }

    /// <summary>
    /// Gets the exact text.
    /// </summary>
    public string Text { get; }
}
