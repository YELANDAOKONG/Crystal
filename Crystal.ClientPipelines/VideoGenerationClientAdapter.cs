using Crystal.Generation;
using Crystal.Generation.Video;
using Crystal.Pipelines;

namespace Crystal.ClientPipelines;

internal sealed class VideoGenerationClientAdapter : IVideoGenerationClient
{
    private readonly AsyncPipeline<VideoGenerationRequest, VideoGenerationResponse> _pipeline;

    public VideoGenerationClientAdapter(
        IVideoGenerationClient client,
        IEnumerable<AsyncMiddleware<VideoGenerationRequest, VideoGenerationResponse>> middleware)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        Capabilities = client.Capabilities;
        _pipeline = new AsyncPipeline<VideoGenerationRequest, VideoGenerationResponse>(
            client.GenerateAsync,
            middleware);
    }

    public GenerationCapabilities Capabilities { get; }

    public Task<VideoGenerationResponse> GenerateAsync(
        VideoGenerationRequest request,
        CancellationToken cancellationToken = default) =>
        _pipeline.InvokeAsync(request, cancellationToken);
}
