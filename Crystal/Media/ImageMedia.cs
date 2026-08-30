namespace Crystal.Media;

/// <summary>Describes one image and its explicit representation.</summary>
public sealed record ImageMedia
{
    /// <summary>Initializes image media.</summary>
    /// <param name="source">The image source.</param>
    /// <param name="mimeType">The exact image MIME type.</param>
    /// <param name="pixelSize">Optional exact pixel dimensions.</param>
    public ImageMedia(
        MediaSource source,
        MediaMimeType mimeType,
        PixelSize? pixelSize = null)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(mimeType, nameof(mimeType));

        Source = source;
        MimeType = mimeType;
        PixelSize = pixelSize;
    }

    /// <summary>Gets the image source.</summary>
    public MediaSource Source { get; }

    /// <summary>Gets the exact image MIME type.</summary>
    public MediaMimeType MimeType { get; }

    /// <summary>Gets exact pixel dimensions when known.</summary>
    public PixelSize? PixelSize { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(ImageMedia);
}
