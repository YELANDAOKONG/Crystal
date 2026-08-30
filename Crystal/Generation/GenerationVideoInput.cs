using Crystal.Media;

namespace Crystal.Generation;

/// <summary>Contains one video generation input with an explicit purpose.</summary>
public sealed record GenerationVideoInput : GenerationInput
{
    /// <summary>Initializes a video generation input.</summary>
    /// <param name="video">The exact video.</param>
    /// <param name="purpose">Its portable conditioning purpose.</param>
    public GenerationVideoInput(
        VideoMedia video,
        GenerationInputPurpose purpose)
    {
        ArgumentNullException.ThrowIfNull(video, nameof(video));
        ArgumentNullException.ThrowIfNull(purpose, nameof(purpose));
        GenerationInputRules.Validate(ContentModality.Video, purpose, nameof(purpose));

        Video = video;
        Purpose = purpose;
    }

    /// <inheritdoc />
    public override ContentModality Modality => ContentModality.Video;

    /// <inheritdoc />
    public override GenerationInputPurpose Purpose { get; }

    /// <summary>Gets the exact video.</summary>
    public VideoMedia Video { get; }
}
