using Crystal.Multimodal.Chat;
using Crystal.Pipelines;

namespace Crystal.Decorators.Chat;

internal sealed class MultimodalChatClientAdapter : IMultimodalChatClient
{
    private readonly AsyncPipeline<MultimodalChatRequest, MultimodalChatResponse> _pipeline;

    public MultimodalChatClientAdapter(
        IMultimodalChatClient client,
        IEnumerable<AsyncMiddleware<MultimodalChatRequest, MultimodalChatResponse>> middleware)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        Capabilities = client.Capabilities;
        _pipeline = new AsyncPipeline<MultimodalChatRequest, MultimodalChatResponse>(
            client.CompleteAsync,
            middleware);
    }

    public MultimodalChatCapabilities Capabilities { get; }

    public Task<MultimodalChatResponse> CompleteAsync(
        MultimodalChatRequest request,
        CancellationToken cancellationToken = default) =>
        _pipeline.InvokeAsync(request, cancellationToken);
}
