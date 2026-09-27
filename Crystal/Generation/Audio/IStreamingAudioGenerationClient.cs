using Crystal.Generation.Streaming;

namespace Crystal.Generation.Audio;

/// <summary>Defines optional audio generation with provisional media output.</summary>
public interface IStreamingAudioGenerationClient
{
    /// <summary>Gets portable audio-generation support.</summary>
    GenerationCapabilities Capabilities { get; }

    /// <summary>Streams provisional media and one final complete response.</summary>
    IAsyncEnumerable<GenerationStreamEvent<AudioGenerationResponse>> StreamAsync(
        AudioGenerationRequest request,
        CancellationToken cancellationToken = default);
}
