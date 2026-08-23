namespace Crystal.Reasoning;

/// <summary>
/// Contains one exact readable reasoning segment.
/// </summary>
public sealed record ReasoningText
{
    /// <summary>
    /// Initializes a readable reasoning segment.
    /// </summary>
    /// <param name="text">The exact model-produced text.</param>
    /// <param name="kind">The classification of the text.</param>
    public ReasoningText(string text, ReasoningTextKind kind)
    {
        ArgumentException.ThrowIfNullOrEmpty(text, nameof(text));
        ArgumentNullException.ThrowIfNull(kind, nameof(kind));

        Text = text;
        Kind = kind;
    }

    /// <summary>
    /// Gets the exact text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the text classification.
    /// </summary>
    public ReasoningTextKind Kind { get; }
}
