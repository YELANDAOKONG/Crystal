using Crystal.Generation;
using Crystal.Generation.Audio;
using Crystal.Generation.Batches;
using Crystal.Generation.Operations;
using Crystal.Pipelines;

namespace Crystal.Decorators.Generation;

internal sealed class AudioGenerationBatchClientAdapter : IAudioGenerationBatchClient
{
    private readonly AsyncPipeline<GenerationBatchRequest<AudioGenerationRequest>,
        GenerationOperationSnapshot<GenerationBatchResponse<AudioGenerationResponse>>> _start;
    private readonly AsyncPipeline<GenerationOperationTicket,
        GenerationOperationSnapshot<GenerationBatchResponse<AudioGenerationResponse>>> _poll;

    public AudioGenerationBatchClientAdapter(
        IAudioGenerationBatchClient client,
        IEnumerable<AsyncMiddleware<GenerationBatchRequest<AudioGenerationRequest>,
            GenerationOperationSnapshot<GenerationBatchResponse<AudioGenerationResponse>>>>
            startMiddleware,
        IEnumerable<AsyncMiddleware<GenerationOperationTicket,
            GenerationOperationSnapshot<GenerationBatchResponse<AudioGenerationResponse>>>>
            pollMiddleware)
    {
        ArgumentNullException.ThrowIfNull(client);
        Capabilities = client.Capabilities;
        _start = new(client.StartBatchAsync, startMiddleware);
        _poll = new(client.PollBatchAsync, pollMiddleware);
    }

    public GenerationCapabilities Capabilities { get; }

    public Task<GenerationOperationSnapshot<GenerationBatchResponse<AudioGenerationResponse>>>
        StartBatchAsync(
            GenerationBatchRequest<AudioGenerationRequest> request,
            CancellationToken cancellationToken = default) =>
        _start.InvokeAsync(request, cancellationToken);

    public Task<GenerationOperationSnapshot<GenerationBatchResponse<AudioGenerationResponse>>>
        PollBatchAsync(
            GenerationOperationTicket ticket,
            CancellationToken cancellationToken = default) =>
        _poll.InvokeAsync(ticket, cancellationToken);
}
