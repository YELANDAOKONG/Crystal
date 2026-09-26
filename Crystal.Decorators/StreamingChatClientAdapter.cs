using Crystal.Chat;
using Crystal.Pipelines;

namespace Crystal.Decorators;

internal sealed class StreamingChatClientAdapter : IStreamingChatClient
{
    private readonly AsyncPipeline<ChatRequest, ChatResponse> _complete;
    private readonly StreamingPipeline<ChatRequest, ChatStreamEvent> _stream;

    public StreamingChatClientAdapter(
        IStreamingChatClient client,
        IEnumerable<AsyncMiddleware<ChatRequest, ChatResponse>> middleware,
        IEnumerable<StreamingMiddleware<ChatRequest, ChatStreamEvent>> streamingMiddleware)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        _complete = new AsyncPipeline<ChatRequest, ChatResponse>(client.CompleteAsync, middleware);
        _stream = new StreamingPipeline<ChatRequest, ChatStreamEvent>(client.StreamAsync, streamingMiddleware);
    }

    public Task<ChatResponse> CompleteAsync(
        ChatRequest request,
        CancellationToken cancellationToken = default) =>
        _complete.InvokeAsync(request, cancellationToken);

    public IAsyncEnumerable<ChatStreamEvent> StreamAsync(
        ChatRequest request,
        CancellationToken cancellationToken = default) =>
        _stream.StreamAsync(request, cancellationToken);
}
