using Crystal.Completions;
using Crystal.Pipelines;

namespace Crystal.Decorators;

internal sealed class CompletionClientAdapter : ICompletionClient
{
    private readonly AsyncPipeline<CompletionRequest, CompletionResponse> _pipeline;

    public CompletionClientAdapter(
        ICompletionClient client,
        IEnumerable<AsyncMiddleware<CompletionRequest, CompletionResponse>> middleware)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        _pipeline = new AsyncPipeline<CompletionRequest, CompletionResponse>(client.CompleteAsync, middleware);
    }

    public Task<CompletionResponse> CompleteAsync(
        CompletionRequest request,
        CancellationToken cancellationToken = default) =>
        _pipeline.InvokeAsync(request, cancellationToken);
}
