using Crystal.Media;

namespace Crystal.Multimodal;

/// <summary>Contains one audio block.</summary>
public sealed record AudioContent : MultimodalContent
{
    /// <summary>Initializes an audio block.</summary>
    /// <param name="audio">The exact audio value.</param>
    public AudioContent(AudioMedia audio)
    {
        ArgumentNullException.ThrowIfNull(audio, nameof(audio));
        Audio = audio;
    }

    /// <inheritdoc />
    public override ContentModality Modality => ContentModality.Audio;

    /// <summary>Gets the exact audio value.</summary>
    public AudioMedia Audio { get; }
}
