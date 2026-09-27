using Crystal.Generation;
using Crystal.Generation.Batches;
using Crystal.Generation.Images;
using Crystal.Generation.Operations;
using Crystal.Pipelines;

namespace Crystal.Decorators.Generation;

internal sealed class ImageGenerationBatchClientAdapter : IImageGenerationBatchClient
{
    private readonly AsyncPipeline<GenerationBatchRequest<ImageGenerationRequest>,
        GenerationOperationSnapshot<GenerationBatchResponse<ImageGenerationResponse>>> _start;
    private readonly AsyncPipeline<GenerationOperationTicket,
        GenerationOperationSnapshot<GenerationBatchResponse<ImageGenerationResponse>>> _poll;

    public ImageGenerationBatchClientAdapter(
        IImageGenerationBatchClient client,
        IEnumerable<AsyncMiddleware<GenerationBatchRequest<ImageGenerationRequest>,
            GenerationOperationSnapshot<GenerationBatchResponse<ImageGenerationResponse>>>>
            startMiddleware,
        IEnumerable<AsyncMiddleware<GenerationOperationTicket,
            GenerationOperationSnapshot<GenerationBatchResponse<ImageGenerationResponse>>>>
            pollMiddleware)
    {
        ArgumentNullException.ThrowIfNull(client);
        Capabilities = client.Capabilities;
        _start = new(client.StartBatchAsync, startMiddleware);
        _poll = new(client.PollBatchAsync, pollMiddleware);
    }

    public GenerationCapabilities Capabilities { get; }

    public Task<GenerationOperationSnapshot<GenerationBatchResponse<ImageGenerationResponse>>>
        StartBatchAsync(
            GenerationBatchRequest<ImageGenerationRequest> request,
            CancellationToken cancellationToken = default) =>
        _start.InvokeAsync(request, cancellationToken);

    public Task<GenerationOperationSnapshot<GenerationBatchResponse<ImageGenerationResponse>>>
        PollBatchAsync(
            GenerationOperationTicket ticket,
            CancellationToken cancellationToken = default) =>
        _poll.InvokeAsync(ticket, cancellationToken);
}
