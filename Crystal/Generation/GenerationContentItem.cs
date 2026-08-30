using Crystal.Multimodal;

namespace Crystal.Generation;

/// <summary>Contains one generated text, image, audio, or video block.</summary>
public sealed record GenerationContentItem : GenerationItem
{
    /// <summary>Initializes a generated content item.</summary>
    /// <param name="content">The exact generated content.</param>
    public GenerationContentItem(MultimodalContent content)
    {
        ArgumentNullException.ThrowIfNull(content, nameof(content));
        Content = content;
    }

    /// <summary>Gets the exact generated content.</summary>
    public MultimodalContent Content { get; }
}
