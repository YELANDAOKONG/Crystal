using Crystal.Generation;
using Crystal.Generation.Audio;
using Crystal.Generation.Operations;
using Crystal.Pipelines;

namespace Crystal.Decorators.Generation;

internal sealed class AudioGenerationOperationClientAdapter : IAudioGenerationOperationClient
{
    private readonly AsyncPipeline<AudioGenerationRequest,
        GenerationOperationSnapshot<AudioGenerationResponse>> _start;
    private readonly AsyncPipeline<GenerationOperationTicket,
        GenerationOperationSnapshot<AudioGenerationResponse>> _poll;

    public AudioGenerationOperationClientAdapter(
        IAudioGenerationOperationClient client,
        IEnumerable<AsyncMiddleware<AudioGenerationRequest,
            GenerationOperationSnapshot<AudioGenerationResponse>>> startMiddleware,
        IEnumerable<AsyncMiddleware<GenerationOperationTicket,
            GenerationOperationSnapshot<AudioGenerationResponse>>> pollMiddleware)
    {
        ArgumentNullException.ThrowIfNull(client);
        Capabilities = client.Capabilities;
        _start = new(client.StartAsync, startMiddleware);
        _poll = new(client.PollAsync, pollMiddleware);
    }

    public GenerationCapabilities Capabilities { get; }

    public Task<GenerationOperationSnapshot<AudioGenerationResponse>> StartAsync(
        AudioGenerationRequest request,
        CancellationToken cancellationToken = default) =>
        _start.InvokeAsync(request, cancellationToken);

    public Task<GenerationOperationSnapshot<AudioGenerationResponse>> PollAsync(
        GenerationOperationTicket ticket,
        CancellationToken cancellationToken = default) =>
        _poll.InvokeAsync(ticket, cancellationToken);
}
