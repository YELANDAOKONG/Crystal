using Crystal.Generation;
using Crystal.Generation.Streaming;
using Crystal.Generation.Video;
using Crystal.Pipelines;

namespace Crystal.Decorators.Generation;

internal sealed class StreamingVideoGenerationClientAdapter : IStreamingVideoGenerationClient
{
    private readonly StreamingPipeline<VideoGenerationRequest,
        GenerationStreamEvent<VideoGenerationResponse>> _pipeline;

    public StreamingVideoGenerationClientAdapter(
        IStreamingVideoGenerationClient client,
        IEnumerable<StreamingMiddleware<VideoGenerationRequest,
            GenerationStreamEvent<VideoGenerationResponse>>> middleware)
    {
        ArgumentNullException.ThrowIfNull(client);
        Capabilities = client.Capabilities;
        _pipeline = new StreamingPipeline<VideoGenerationRequest,
            GenerationStreamEvent<VideoGenerationResponse>>(
            client.StreamAsync,
            middleware);
    }

    public GenerationCapabilities Capabilities { get; }

    public IAsyncEnumerable<GenerationStreamEvent<VideoGenerationResponse>>
        StreamAsync(
            VideoGenerationRequest request,
            CancellationToken cancellationToken = default) =>
        _pipeline.StreamAsync(request, cancellationToken);
}
