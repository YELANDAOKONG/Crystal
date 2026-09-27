using Crystal.Generation;
using Crystal.Generation.Images;
using Crystal.Generation.Operations;
using Crystal.Pipelines;

namespace Crystal.Decorators.Generation;

internal sealed class ImageGenerationOperationClientAdapter : IImageGenerationOperationClient
{
    private readonly AsyncPipeline<ImageGenerationRequest,
        GenerationOperationSnapshot<ImageGenerationResponse>> _start;
    private readonly AsyncPipeline<GenerationOperationTicket,
        GenerationOperationSnapshot<ImageGenerationResponse>> _poll;

    public ImageGenerationOperationClientAdapter(
        IImageGenerationOperationClient client,
        IEnumerable<AsyncMiddleware<ImageGenerationRequest,
            GenerationOperationSnapshot<ImageGenerationResponse>>> startMiddleware,
        IEnumerable<AsyncMiddleware<GenerationOperationTicket,
            GenerationOperationSnapshot<ImageGenerationResponse>>> pollMiddleware)
    {
        ArgumentNullException.ThrowIfNull(client);
        Capabilities = client.Capabilities;
        _start = new(client.StartAsync, startMiddleware);
        _poll = new(client.PollAsync, pollMiddleware);
    }

    public GenerationCapabilities Capabilities { get; }

    public Task<GenerationOperationSnapshot<ImageGenerationResponse>> StartAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default) =>
        _start.InvokeAsync(request, cancellationToken);

    public Task<GenerationOperationSnapshot<ImageGenerationResponse>> PollAsync(
        GenerationOperationTicket ticket,
        CancellationToken cancellationToken = default) =>
        _poll.InvokeAsync(ticket, cancellationToken);
}
