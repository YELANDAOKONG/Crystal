using Crystal.Embeddings;
using Crystal.Pipelines;

namespace Crystal.Decorators;

internal sealed class EmbeddingClientAdapter : IEmbeddingClient
{
    private readonly AsyncPipeline<EmbeddingRequest, EmbeddingResponse> _pipeline;

    public EmbeddingClientAdapter(
        IEmbeddingClient client,
        IEnumerable<AsyncMiddleware<EmbeddingRequest, EmbeddingResponse>> middleware)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        _pipeline = new AsyncPipeline<EmbeddingRequest, EmbeddingResponse>(client.EmbedAsync, middleware);
    }

    public Task<EmbeddingResponse> EmbedAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken = default) =>
        _pipeline.InvokeAsync(request, cancellationToken);
}
