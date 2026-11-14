namespace Crystal.Media;

/// <summary>Describes a complete file with an explicit representation.</summary>
public sealed record FileMedia
{
    /// <summary>Initializes file media.</summary>
    /// <param name="source">The source of the complete file.</param>
    /// <param name="mimeType">The exact file MIME type.</param>
    /// <param name="fileName">The optional file name supplied with the content.</param>
    public FileMedia(
        MediaSource source,
        MediaMimeType mimeType,
        string? fileName = null)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(mimeType, nameof(mimeType));

        if (fileName is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fileName, nameof(fileName));
        }

        Source = source;
        MimeType = mimeType;
        FileName = fileName;
    }

    /// <summary>Gets the source of the complete file.</summary>
    public MediaSource Source { get; }

    /// <summary>Gets the exact file MIME type.</summary>
    public MediaMimeType MimeType { get; }

    /// <summary>Gets the supplied file name without using it to locate the source.</summary>
    public string? FileName { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(FileMedia);
}
