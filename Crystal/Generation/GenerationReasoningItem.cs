using Crystal.Multimodal;

namespace Crystal.Generation;

/// <summary>Contains one generated readable or opaque reasoning block.</summary>
public sealed record GenerationReasoningItem : GenerationItem
{
    /// <summary>Initializes a generated reasoning item.</summary>
    /// <param name="content">The exact generated reasoning content.</param>
    public GenerationReasoningItem(MultimodalReasoningContent content)
    {
        ArgumentNullException.ThrowIfNull(content, nameof(content));
        if (content.Parts.Any(static part => part.Content is FileContent))
        {
            throw new ArgumentException("Generation does not produce file reasoning parts.", nameof(content));
        }

        Content = content;
    }

    /// <summary>Gets the exact generated reasoning content.</summary>
    public MultimodalReasoningContent Content { get; }
}
