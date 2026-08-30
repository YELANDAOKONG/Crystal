using Crystal.Media;

namespace Crystal.Multimodal;

/// <summary>Contains one video block.</summary>
public sealed record VideoContent : MultimodalContent
{
    /// <summary>Initializes a video block.</summary>
    /// <param name="video">The exact video value.</param>
    public VideoContent(VideoMedia video)
    {
        ArgumentNullException.ThrowIfNull(video, nameof(video));
        Video = video;
    }

    /// <inheritdoc />
    public override ContentModality Modality => ContentModality.Video;

    /// <summary>Gets the exact video value.</summary>
    public VideoMedia Video { get; }
}
