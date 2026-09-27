using Crystal.Generation.Streaming;

namespace Crystal.Generation.Images;

/// <summary>Defines optional image generation with provisional media output.</summary>
public interface IStreamingImageGenerationClient
{
    /// <summary>Gets portable image-generation support.</summary>
    GenerationCapabilities Capabilities { get; }

    /// <summary>Streams provisional media and one final complete response.</summary>
    IAsyncEnumerable<GenerationStreamEvent<ImageGenerationResponse>> StreamAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default);
}
