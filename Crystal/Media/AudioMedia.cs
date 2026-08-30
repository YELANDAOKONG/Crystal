namespace Crystal.Media;

/// <summary>Describes one audio value and its explicit representation.</summary>
public sealed record AudioMedia
{
    /// <summary>Initializes audio media.</summary>
    /// <param name="source">The audio source.</param>
    /// <param name="mimeType">The exact audio MIME type.</param>
    /// <param name="codec">The optional exact codec identifier.</param>
    /// <param name="duration">The optional positive duration.</param>
    /// <param name="sampleRate">The optional positive sample rate in hertz.</param>
    /// <param name="channelCount">The optional positive channel count.</param>
    public AudioMedia(
        MediaSource source,
        MediaMimeType mimeType,
        MediaCodec? codec = null,
        TimeSpan? duration = null,
        int? sampleRate = null,
        int? channelCount = null)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(mimeType, nameof(mimeType));

        if (duration is not null && duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(duration),
                duration,
                "Audio duration must be positive.");
        }

        if (sampleRate is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sampleRate),
                sampleRate,
                "Audio sample rate must be positive.");
        }

        if (channelCount is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(channelCount),
                channelCount,
                "Audio channel count must be positive.");
        }

        Source = source;
        MimeType = mimeType;
        Codec = codec;
        Duration = duration;
        SampleRate = sampleRate;
        ChannelCount = channelCount;
    }

    /// <summary>Gets the audio source.</summary>
    public MediaSource Source { get; }

    /// <summary>Gets the exact audio MIME type.</summary>
    public MediaMimeType MimeType { get; }

    /// <summary>Gets the exact codec identifier when known.</summary>
    public MediaCodec? Codec { get; }

    /// <summary>Gets the duration when known.</summary>
    public TimeSpan? Duration { get; }

    /// <summary>Gets the sample rate in hertz when known.</summary>
    public int? SampleRate { get; }

    /// <summary>Gets the channel count when known.</summary>
    public int? ChannelCount { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(AudioMedia);
}
