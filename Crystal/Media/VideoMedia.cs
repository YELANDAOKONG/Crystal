namespace Crystal.Media;

/// <summary>Describes one video value and its explicit representation.</summary>
public sealed record VideoMedia
{
    /// <summary>Initializes video media.</summary>
    /// <param name="source">The video source.</param>
    /// <param name="mimeType">The exact video MIME type.</param>
    /// <param name="codec">The optional exact video-codec identifier.</param>
    /// <param name="pixelSize">Optional exact frame dimensions.</param>
    /// <param name="duration">The optional positive duration.</param>
    /// <param name="frameRate">The optional positive frame rate.</param>
    /// <param name="audioPresence">Embedded-audio presence.</param>
    public VideoMedia(
        MediaSource source,
        MediaMimeType mimeType,
        MediaCodec? codec = null,
        PixelSize? pixelSize = null,
        TimeSpan? duration = null,
        decimal? frameRate = null,
        VideoAudioPresence? audioPresence = null)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(mimeType, nameof(mimeType));

        if (duration is not null && duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(duration),
                duration,
                "Video duration must be positive.");
        }

        if (frameRate is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(frameRate),
                frameRate,
                "Video frame rate must be positive.");
        }

        Source = source;
        MimeType = mimeType;
        Codec = codec;
        PixelSize = pixelSize;
        Duration = duration;
        FrameRate = frameRate;
        AudioPresence = audioPresence ?? VideoAudioPresence.Unknown;
    }

    /// <summary>Gets the video source.</summary>
    public MediaSource Source { get; }

    /// <summary>Gets the exact video MIME type.</summary>
    public MediaMimeType MimeType { get; }

    /// <summary>Gets the exact video-codec identifier when known.</summary>
    public MediaCodec? Codec { get; }

    /// <summary>Gets exact frame dimensions when known.</summary>
    public PixelSize? PixelSize { get; }

    /// <summary>Gets the duration when known.</summary>
    public TimeSpan? Duration { get; }

    /// <summary>Gets the frame rate when known.</summary>
    public decimal? FrameRate { get; }

    /// <summary>Gets embedded-audio presence.</summary>
    public VideoAudioPresence AudioPresence { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(VideoMedia);
}
