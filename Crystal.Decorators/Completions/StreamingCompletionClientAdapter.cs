using Crystal.Completions;
using Crystal.Pipelines;

namespace Crystal.Decorators.Completions;

internal sealed class StreamingCompletionClientAdapter : IStreamingCompletionClient
{
    private readonly AsyncPipeline<CompletionRequest, CompletionResponse> _complete;
    private readonly StreamingPipeline<CompletionRequest, CompletionStreamEvent> _stream;

    public StreamingCompletionClientAdapter(
        IStreamingCompletionClient client,
        IEnumerable<AsyncMiddleware<CompletionRequest, CompletionResponse>> middleware,
        IEnumerable<StreamingMiddleware<CompletionRequest, CompletionStreamEvent>> streamingMiddleware)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        _complete = new AsyncPipeline<CompletionRequest, CompletionResponse>(client.CompleteAsync, middleware);
        _stream = new StreamingPipeline<CompletionRequest, CompletionStreamEvent>(client.StreamAsync, streamingMiddleware);
    }

    public Task<CompletionResponse> CompleteAsync(
        CompletionRequest request,
        CancellationToken cancellationToken = default) =>
        _complete.InvokeAsync(request, cancellationToken);

    public IAsyncEnumerable<CompletionStreamEvent> StreamAsync(
        CompletionRequest request,
        CancellationToken cancellationToken = default) =>
        _stream.StreamAsync(request, cancellationToken);
}
