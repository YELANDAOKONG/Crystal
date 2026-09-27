using Crystal.Generation;
using Crystal.Generation.Images;
using Crystal.Pipelines;

namespace Crystal.Decorators.Generation;

internal sealed class ImageGenerationClientAdapter : IImageGenerationClient
{
    private readonly AsyncPipeline<ImageGenerationRequest, ImageGenerationResponse> _pipeline;

    public ImageGenerationClientAdapter(
        IImageGenerationClient client,
        IEnumerable<AsyncMiddleware<ImageGenerationRequest, ImageGenerationResponse>> middleware)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        Capabilities = client.Capabilities;
        _pipeline = new AsyncPipeline<ImageGenerationRequest, ImageGenerationResponse>(
            client.GenerateAsync,
            middleware);
    }

    public GenerationCapabilities Capabilities { get; }

    public Task<ImageGenerationResponse> GenerateAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default) =>
        _pipeline.InvokeAsync(request, cancellationToken);
}
