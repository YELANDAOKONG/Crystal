using Crystal.Generation;
using Crystal.Generation.Streaming;
using Crystal.Generation.Video;
using Crystal.Pipelines;

namespace Crystal.Decorators.Generation;

internal sealed class CombinedVideoGenerationClientAdapter :
    IVideoGenerationClient,
    IStreamingVideoGenerationClient
{
    private readonly AsyncPipeline<VideoGenerationRequest, VideoGenerationResponse> _generate;
    private readonly StreamingPipeline<VideoGenerationRequest,
        GenerationStreamEvent<VideoGenerationResponse>> _stream;
    private readonly GenerationCapabilities _streamCapabilities;

    public CombinedVideoGenerationClientAdapter(
        IVideoGenerationClient client,
        IStreamingVideoGenerationClient streamingClient,
        IEnumerable<AsyncMiddleware<VideoGenerationRequest,
            VideoGenerationResponse>> middleware,
        IEnumerable<StreamingMiddleware<VideoGenerationRequest,
            GenerationStreamEvent<VideoGenerationResponse>>> streamingMiddleware)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(streamingClient);
        Capabilities = client.Capabilities;
        _streamCapabilities = streamingClient.Capabilities;
        _generate = new(client.GenerateAsync, middleware);
        _stream = new(streamingClient.StreamAsync, streamingMiddleware);
    }

    public GenerationCapabilities Capabilities { get; }

    GenerationCapabilities IStreamingVideoGenerationClient.Capabilities =>
        _streamCapabilities;

    public Task<VideoGenerationResponse> GenerateAsync(
        VideoGenerationRequest request,
        CancellationToken cancellationToken = default) =>
        _generate.InvokeAsync(request, cancellationToken);

    public IAsyncEnumerable<GenerationStreamEvent<VideoGenerationResponse>>
        StreamAsync(
            VideoGenerationRequest request,
            CancellationToken cancellationToken = default) =>
        _stream.StreamAsync(request, cancellationToken);
}
