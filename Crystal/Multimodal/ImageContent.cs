using Crystal.Media;

namespace Crystal.Multimodal;

/// <summary>Contains one image block.</summary>
public sealed record ImageContent : MultimodalContent
{
    /// <summary>Initializes an image block.</summary>
    /// <param name="image">The exact image value.</param>
    public ImageContent(ImageMedia image)
    {
        ArgumentNullException.ThrowIfNull(image, nameof(image));
        Image = image;
    }

    /// <inheritdoc />
    public override ContentModality Modality => ContentModality.Image;

    /// <summary>Gets the exact image value.</summary>
    public ImageMedia Image { get; }
}
