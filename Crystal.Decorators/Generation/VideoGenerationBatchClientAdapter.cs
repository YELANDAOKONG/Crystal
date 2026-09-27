using Crystal.Generation;
using Crystal.Generation.Batches;
using Crystal.Generation.Operations;
using Crystal.Generation.Video;
using Crystal.Pipelines;

namespace Crystal.Decorators.Generation;

internal sealed class VideoGenerationBatchClientAdapter : IVideoGenerationBatchClient
{
    private readonly AsyncPipeline<GenerationBatchRequest<VideoGenerationRequest>,
        GenerationOperationSnapshot<GenerationBatchResponse<VideoGenerationResponse>>> _start;
    private readonly AsyncPipeline<GenerationOperationTicket,
        GenerationOperationSnapshot<GenerationBatchResponse<VideoGenerationResponse>>> _poll;

    public VideoGenerationBatchClientAdapter(
        IVideoGenerationBatchClient client,
        IEnumerable<AsyncMiddleware<GenerationBatchRequest<VideoGenerationRequest>,
            GenerationOperationSnapshot<GenerationBatchResponse<VideoGenerationResponse>>>>
            startMiddleware,
        IEnumerable<AsyncMiddleware<GenerationOperationTicket,
            GenerationOperationSnapshot<GenerationBatchResponse<VideoGenerationResponse>>>>
            pollMiddleware)
    {
        ArgumentNullException.ThrowIfNull(client);
        Capabilities = client.Capabilities;
        _start = new(client.StartBatchAsync, startMiddleware);
        _poll = new(client.PollBatchAsync, pollMiddleware);
    }

    public GenerationCapabilities Capabilities { get; }

    public Task<GenerationOperationSnapshot<GenerationBatchResponse<VideoGenerationResponse>>>
        StartBatchAsync(
            GenerationBatchRequest<VideoGenerationRequest> request,
            CancellationToken cancellationToken = default) =>
        _start.InvokeAsync(request, cancellationToken);

    public Task<GenerationOperationSnapshot<GenerationBatchResponse<VideoGenerationResponse>>>
        PollBatchAsync(
            GenerationOperationTicket ticket,
            CancellationToken cancellationToken = default) =>
        _poll.InvokeAsync(ticket, cancellationToken);
}
