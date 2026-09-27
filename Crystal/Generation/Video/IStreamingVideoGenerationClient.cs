using Crystal.Generation.Streaming;

namespace Crystal.Generation.Video;

/// <summary>Defines optional video generation with provisional media output.</summary>
public interface IStreamingVideoGenerationClient
{
    /// <summary>Gets portable video-generation support.</summary>
    GenerationCapabilities Capabilities { get; }

    /// <summary>Streams provisional media and one final complete response.</summary>
    IAsyncEnumerable<GenerationStreamEvent<VideoGenerationResponse>> StreamAsync(
        VideoGenerationRequest request,
        CancellationToken cancellationToken = default);
}
