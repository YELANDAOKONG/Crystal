using Crystal.Media;

namespace Crystal.Generation;

/// <summary>Contains one image generation input with an explicit purpose.</summary>
public sealed record GenerationImageInput : GenerationInput
{
    /// <summary>Initializes an image generation input.</summary>
    /// <param name="image">The exact image.</param>
    /// <param name="purpose">Its portable conditioning purpose.</param>
    public GenerationImageInput(
        ImageMedia image,
        GenerationInputPurpose purpose)
    {
        ArgumentNullException.ThrowIfNull(image, nameof(image));
        ArgumentNullException.ThrowIfNull(purpose, nameof(purpose));
        GenerationInputRules.Validate(ContentModality.Image, purpose, nameof(purpose));

        Image = image;
        Purpose = purpose;
    }

    /// <inheritdoc />
    public override ContentModality Modality => ContentModality.Image;

    /// <inheritdoc />
    public override GenerationInputPurpose Purpose { get; }

    /// <summary>Gets the exact image.</summary>
    public ImageMedia Image { get; }
}
