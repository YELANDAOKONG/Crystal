using Crystal.Media;

namespace Crystal.Multimodal;

/// <summary>Contains one complete file block.</summary>
public sealed record FileContent : MultimodalContent
{
    /// <summary>Initializes a file block.</summary>
    /// <param name="file">The exact file value.</param>
    public FileContent(FileMedia file)
    {
        ArgumentNullException.ThrowIfNull(file, nameof(file));
        File = file;
    }

    /// <inheritdoc />
    public override ContentModality Modality => ContentModality.File;

    /// <summary>Gets the exact file value.</summary>
    public FileMedia File { get; }
}
