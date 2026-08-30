namespace Crystal.Multimodal;

/// <summary>Contains one exact text block.</summary>
public sealed record TextContent : MultimodalContent
{
    /// <summary>Initializes an exact text block.</summary>
    /// <param name="text">The exact text, including an empty value.</param>
    public TextContent(string text)
    {
        ArgumentNullException.ThrowIfNull(text, nameof(text));
        Text = text;
    }

    /// <inheritdoc />
    public override ContentModality Modality => ContentModality.Text;

    /// <summary>Gets the exact text.</summary>
    public string Text { get; }
}
