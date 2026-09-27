using Crystal.Generation;
using Crystal.Generation.Images;
using Crystal.Generation.Streaming;
using Crystal.Pipelines;

namespace Crystal.Decorators.Generation;

internal sealed class CombinedImageGenerationClientAdapter :
    IImageGenerationClient,
    IStreamingImageGenerationClient
{
    private readonly StreamingPipeline<ImageGenerationRequest,
        GenerationStreamEvent<ImageGenerationResponse>> _stream;
    private readonly AsyncPipeline<ImageGenerationRequest, ImageGenerationResponse> _generate;
    private readonly GenerationCapabilities _streamCapabilities;

    public CombinedImageGenerationClientAdapter(
        IImageGenerationClient client,
        IStreamingImageGenerationClient streamingClient,
        IEnumerable<AsyncMiddleware<ImageGenerationRequest,
            ImageGenerationResponse>> middleware,
        IEnumerable<StreamingMiddleware<ImageGenerationRequest,
            GenerationStreamEvent<ImageGenerationResponse>>> streamingMiddleware)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(streamingClient);
        Capabilities = client.Capabilities;
        _streamCapabilities = streamingClient.Capabilities;
        _generate = new(client.GenerateAsync, middleware);
        _stream = new(streamingClient.StreamAsync, streamingMiddleware);
    }

    public GenerationCapabilities Capabilities { get; }

    GenerationCapabilities IStreamingImageGenerationClient.Capabilities =>
        _streamCapabilities;

    public Task<ImageGenerationResponse> GenerateAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default) =>
        _generate.InvokeAsync(request, cancellationToken);

    public IAsyncEnumerable<GenerationStreamEvent<ImageGenerationResponse>>
        StreamAsync(
            ImageGenerationRequest request,
            CancellationToken cancellationToken = default) =>
        _stream.StreamAsync(request, cancellationToken);
}
