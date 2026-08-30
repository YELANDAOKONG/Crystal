using Crystal.Multimodal;

namespace Crystal.Generation;

/// <summary>Describes one supported generation input shape.</summary>
public sealed record GenerationInputCapability
{
    /// <summary>Initializes a supported generation input shape.</summary>
    /// <param name="content">The supported content and source shapes.</param>
    /// <param name="purpose">The supported portable purpose.</param>
    public GenerationInputCapability(
        MultimodalContentCapability content,
        GenerationInputPurpose purpose)
    {
        ArgumentNullException.ThrowIfNull(content, nameof(content));
        ArgumentNullException.ThrowIfNull(purpose, nameof(purpose));
        GenerationInputRules.Validate(content.Modality, purpose, nameof(purpose));

        Content = content;
        Purpose = purpose;
    }

    /// <summary>Gets the supported content and source shapes.</summary>
    public MultimodalContentCapability Content { get; }

    /// <summary>Gets the supported portable purpose.</summary>
    public GenerationInputPurpose Purpose { get; }
}
