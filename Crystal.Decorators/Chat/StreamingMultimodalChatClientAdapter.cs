using Crystal.Multimodal.Chat;
using Crystal.Pipelines;

namespace Crystal.Decorators.Chat;

internal sealed class StreamingMultimodalChatClientAdapter : IStreamingMultimodalChatClient
{
    private readonly AsyncPipeline<MultimodalChatRequest, MultimodalChatResponse> _complete;
    private readonly StreamingPipeline<MultimodalChatRequest, MultimodalChatStreamEvent> _stream;

    public StreamingMultimodalChatClientAdapter(
        IStreamingMultimodalChatClient client,
        IEnumerable<AsyncMiddleware<MultimodalChatRequest, MultimodalChatResponse>> middleware,
        IEnumerable<StreamingMiddleware<MultimodalChatRequest, MultimodalChatStreamEvent>> streamingMiddleware)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        Capabilities = client.Capabilities;
        _complete = new AsyncPipeline<MultimodalChatRequest, MultimodalChatResponse>(
            client.CompleteAsync,
            middleware);
        _stream = new StreamingPipeline<MultimodalChatRequest, MultimodalChatStreamEvent>(
            client.StreamAsync,
            streamingMiddleware);
    }

    public MultimodalChatCapabilities Capabilities { get; }

    public Task<MultimodalChatResponse> CompleteAsync(
        MultimodalChatRequest request,
        CancellationToken cancellationToken = default) =>
        _complete.InvokeAsync(request, cancellationToken);

    public IAsyncEnumerable<MultimodalChatStreamEvent> StreamAsync(
        MultimodalChatRequest request,
        CancellationToken cancellationToken = default) =>
        _stream.StreamAsync(request, cancellationToken);
}
