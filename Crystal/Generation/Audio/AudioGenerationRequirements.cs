using Crystal.Media;

namespace Crystal.Generation.Audio;

/// <summary>Contains portable hard requirements for generated audio.</summary>
public sealed record AudioGenerationRequirements
{
    /// <summary>Initializes audio generation requirements.</summary>
    /// <param name="mimeType">The optional required output MIME type.</param>
    /// <param name="sourceKind">The optional required output source shape.</param>
    /// <param name="codec">The optional required codec.</param>
    /// <param name="duration">The optional required positive duration.</param>
    /// <param name="sampleRate">The optional required sample rate in hertz.</param>
    /// <param name="channelCount">The optional required channel count.</param>
    public AudioGenerationRequirements(
        MediaMimeType? mimeType = null,
        MediaSourceKind? sourceKind = null,
        MediaCodec? codec = null,
        TimeSpan? duration = null,
        int? sampleRate = null,
        int? channelCount = null)
    {
        if (duration is not null && duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(duration),
                duration,
                "Required audio duration must be positive.");
        }

        if (sampleRate is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sampleRate),
                sampleRate,
                "Required audio sample rate must be positive.");
        }

        if (channelCount is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(channelCount),
                channelCount,
                "Required audio channel count must be positive.");
        }

        MimeType = mimeType;
        SourceKind = sourceKind;
        Codec = codec;
        Duration = duration;
        SampleRate = sampleRate;
        ChannelCount = channelCount;
    }

    /// <summary>Gets the required output MIME type.</summary>
    public MediaMimeType? MimeType { get; }

    /// <summary>Gets the required output source shape.</summary>
    public MediaSourceKind? SourceKind { get; }

    /// <summary>Gets the required codec.</summary>
    public MediaCodec? Codec { get; }

    /// <summary>Gets the required duration.</summary>
    public TimeSpan? Duration { get; }

    /// <summary>Gets the required sample rate in hertz.</summary>
    public int? SampleRate { get; }

    /// <summary>Gets the required channel count.</summary>
    public int? ChannelCount { get; }
}
