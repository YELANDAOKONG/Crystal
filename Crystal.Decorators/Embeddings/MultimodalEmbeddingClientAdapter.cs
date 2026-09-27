using Crystal.Multimodal.Embeddings;
using Crystal.Pipelines;

namespace Crystal.Decorators.Embeddings;

internal sealed class MultimodalEmbeddingClientAdapter : IMultimodalEmbeddingClient
{
    private readonly AsyncPipeline<MultimodalEmbeddingRequest, MultimodalEmbeddingResponse>
        _pipeline;

    public MultimodalEmbeddingClientAdapter(
        IMultimodalEmbeddingClient client,
        IEnumerable<AsyncMiddleware<MultimodalEmbeddingRequest,
            MultimodalEmbeddingResponse>> middleware)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        Capabilities = client.Capabilities;
        _pipeline = new AsyncPipeline<MultimodalEmbeddingRequest,
            MultimodalEmbeddingResponse>(client.EmbedAsync, middleware);
    }

    public MultimodalEmbeddingCapabilities Capabilities { get; }

    public Task<MultimodalEmbeddingResponse> EmbedAsync(
        MultimodalEmbeddingRequest request,
        CancellationToken cancellationToken = default) =>
        _pipeline.InvokeAsync(request, cancellationToken);
}
