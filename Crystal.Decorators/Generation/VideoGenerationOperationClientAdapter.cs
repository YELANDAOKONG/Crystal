using Crystal.Generation;
using Crystal.Generation.Operations;
using Crystal.Generation.Video;
using Crystal.Pipelines;

namespace Crystal.Decorators.Generation;

internal sealed class VideoGenerationOperationClientAdapter : IVideoGenerationOperationClient
{
    private readonly AsyncPipeline<VideoGenerationRequest,
        GenerationOperationSnapshot<VideoGenerationResponse>> _start;
    private readonly AsyncPipeline<GenerationOperationTicket,
        GenerationOperationSnapshot<VideoGenerationResponse>> _poll;

    public VideoGenerationOperationClientAdapter(
        IVideoGenerationOperationClient client,
        IEnumerable<AsyncMiddleware<VideoGenerationRequest,
            GenerationOperationSnapshot<VideoGenerationResponse>>> startMiddleware,
        IEnumerable<AsyncMiddleware<GenerationOperationTicket,
            GenerationOperationSnapshot<VideoGenerationResponse>>> pollMiddleware)
    {
        ArgumentNullException.ThrowIfNull(client);
        Capabilities = client.Capabilities;
        _start = new(client.StartAsync, startMiddleware);
        _poll = new(client.PollAsync, pollMiddleware);
    }

    public GenerationCapabilities Capabilities { get; }

    public Task<GenerationOperationSnapshot<VideoGenerationResponse>> StartAsync(
        VideoGenerationRequest request,
        CancellationToken cancellationToken = default) =>
        _start.InvokeAsync(request, cancellationToken);

    public Task<GenerationOperationSnapshot<VideoGenerationResponse>> PollAsync(
        GenerationOperationTicket ticket,
        CancellationToken cancellationToken = default) =>
        _poll.InvokeAsync(ticket, cancellationToken);
}
