using Crystal.Media;

namespace Crystal.Generation;

/// <summary>Contains one audio generation input with an explicit purpose.</summary>
public sealed record GenerationAudioInput : GenerationInput
{
    /// <summary>Initializes an audio generation input.</summary>
    /// <param name="audio">The exact audio.</param>
    /// <param name="purpose">Its portable conditioning purpose.</param>
    public GenerationAudioInput(
        AudioMedia audio,
        GenerationInputPurpose purpose)
    {
        ArgumentNullException.ThrowIfNull(audio, nameof(audio));
        ArgumentNullException.ThrowIfNull(purpose, nameof(purpose));
        GenerationInputRules.Validate(ContentModality.Audio, purpose, nameof(purpose));

        Audio = audio;
        Purpose = purpose;
    }

    /// <inheritdoc />
    public override ContentModality Modality => ContentModality.Audio;

    /// <inheritdoc />
    public override GenerationInputPurpose Purpose { get; }

    /// <summary>Gets the exact audio.</summary>
    public AudioMedia Audio { get; }
}
