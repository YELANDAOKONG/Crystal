using Crystal.Media;

namespace Crystal.Generation.Images;

/// <summary>Contains portable hard requirements for generated images.</summary>
public sealed record ImageGenerationRequirements
{
    /// <summary>Initializes image generation requirements.</summary>
    /// <param name="mimeType">The optional required output MIME type.</param>
    /// <param name="sourceKind">The optional required output source shape.</param>
    /// <param name="pixelSize">Optional required exact pixel dimensions.</param>
    /// <param name="aspectRatio">Optional required normalized aspect ratio.</param>
    public ImageGenerationRequirements(
        MediaMimeType? mimeType = null,
        MediaSourceKind? sourceKind = null,
        PixelSize? pixelSize = null,
        AspectRatio? aspectRatio = null)
    {
        if (pixelSize is not null
            && aspectRatio is not null
            && (long)pixelSize.Width * aspectRatio.Height
                != (long)pixelSize.Height * aspectRatio.Width)
        {
            throw new ArgumentException(
                "Exact pixel size and aspect ratio must be consistent.",
                nameof(aspectRatio));
        }

        MimeType = mimeType;
        SourceKind = sourceKind;
        PixelSize = pixelSize;
        AspectRatio = aspectRatio;
    }

    /// <summary>Gets the required output MIME type.</summary>
    public MediaMimeType? MimeType { get; }

    /// <summary>Gets the required output source shape.</summary>
    public MediaSourceKind? SourceKind { get; }

    /// <summary>Gets required exact pixel dimensions.</summary>
    public PixelSize? PixelSize { get; }

    /// <summary>Gets the required normalized aspect ratio.</summary>
    public AspectRatio? AspectRatio { get; }
}
