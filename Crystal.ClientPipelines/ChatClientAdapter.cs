using Crystal.Chat;
using Crystal.Pipelines;

namespace Crystal.ClientPipelines;

internal sealed class ChatClientAdapter : IChatClient
{
    private readonly AsyncPipeline<ChatRequest, ChatResponse> _pipeline;

    public ChatClientAdapter(
        IChatClient client,
        IEnumerable<AsyncMiddleware<ChatRequest, ChatResponse>> middleware)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        _pipeline = new AsyncPipeline<ChatRequest, ChatResponse>(client.CompleteAsync, middleware);
    }

    public Task<ChatResponse> CompleteAsync(
        ChatRequest request,
        CancellationToken cancellationToken = default) =>
        _pipeline.InvokeAsync(request, cancellationToken);
}
