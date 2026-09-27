using Crystal.Generation;
using Crystal.Generation.Images;
using Crystal.Generation.Streaming;
using Crystal.Pipelines;

namespace Crystal.Decorators.Generation;

internal sealed class StreamingImageGenerationClientAdapter : IStreamingImageGenerationClient
{
    private readonly StreamingPipeline<ImageGenerationRequest,
        GenerationStreamEvent<ImageGenerationResponse>> _pipeline;

    public StreamingImageGenerationClientAdapter(
        IStreamingImageGenerationClient client,
        IEnumerable<StreamingMiddleware<ImageGenerationRequest,
            GenerationStreamEvent<ImageGenerationResponse>>> middleware)
    {
        ArgumentNullException.ThrowIfNull(client);
        Capabilities = client.Capabilities;
        _pipeline = new StreamingPipeline<ImageGenerationRequest,
            GenerationStreamEvent<ImageGenerationResponse>>(
            client.StreamAsync,
            middleware);
    }

    public GenerationCapabilities Capabilities { get; }

    public IAsyncEnumerable<GenerationStreamEvent<ImageGenerationResponse>>
        StreamAsync(
            ImageGenerationRequest request,
            CancellationToken cancellationToken = default) =>
        _pipeline.StreamAsync(request, cancellationToken);
}
