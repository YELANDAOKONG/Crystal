using Crystal.Media;

namespace Crystal.Generation.Video;

/// <summary>Contains portable hard requirements for generated video.</summary>
public sealed record VideoGenerationRequirements
{
    /// <summary>Initializes video generation requirements.</summary>
    /// <param name="mimeType">The optional required output MIME type.</param>
    /// <param name="sourceKind">The optional required output source shape.</param>
    /// <param name="codec">The optional required video codec.</param>
    /// <param name="pixelSize">Optional required exact frame dimensions.</param>
    /// <param name="aspectRatio">Optional required normalized aspect ratio.</param>
    /// <param name="duration">The optional required positive duration.</param>
    /// <param name="frameRate">The optional required positive frame rate.</param>
    /// <param name="audio">The embedded-audio requirement.</param>
    public VideoGenerationRequirements(
        MediaMimeType? mimeType = null,
        MediaSourceKind? sourceKind = null,
        MediaCodec? codec = null,
        PixelSize? pixelSize = null,
        AspectRatio? aspectRatio = null,
        TimeSpan? duration = null,
        decimal? frameRate = null,
        VideoAudioRequirement? audio = null)
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

        if (duration is not null && duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(duration),
                duration,
                "Required video duration must be positive.");
        }

        if (frameRate is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(frameRate),
                frameRate,
                "Required video frame rate must be positive.");
        }

        MimeType = mimeType;
        SourceKind = sourceKind;
        Codec = codec;
        PixelSize = pixelSize;
        AspectRatio = aspectRatio;
        Duration = duration;
        FrameRate = frameRate;
        Audio = audio ?? VideoAudioRequirement.Unspecified;
    }

    /// <summary>Gets the required output MIME type.</summary>
    public MediaMimeType? MimeType { get; }

    /// <summary>Gets the required output source shape.</summary>
    public MediaSourceKind? SourceKind { get; }

    /// <summary>Gets the required video codec.</summary>
    public MediaCodec? Codec { get; }

    /// <summary>Gets required exact frame dimensions.</summary>
    public PixelSize? PixelSize { get; }

    /// <summary>Gets the required normalized aspect ratio.</summary>
    public AspectRatio? AspectRatio { get; }

    /// <summary>Gets the required duration.</summary>
    public TimeSpan? Duration { get; }

    /// <summary>Gets the required frame rate.</summary>
    public decimal? FrameRate { get; }

    /// <summary>Gets the embedded-audio requirement.</summary>
    public VideoAudioRequirement Audio { get; }
}
