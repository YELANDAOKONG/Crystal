using Crystal.Generation;
using Crystal.Generation.Audio;
using Crystal.Pipelines;

namespace Crystal.Decorators.Generation;

internal sealed class AudioGenerationClientAdapter : IAudioGenerationClient
{
    private readonly AsyncPipeline<AudioGenerationRequest, AudioGenerationResponse> _pipeline;

    public AudioGenerationClientAdapter(
        IAudioGenerationClient client,
        IEnumerable<AsyncMiddleware<AudioGenerationRequest, AudioGenerationResponse>> middleware)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        Capabilities = client.Capabilities;
        _pipeline = new AsyncPipeline<AudioGenerationRequest, AudioGenerationResponse>(
            client.GenerateAsync,
            middleware);
    }

    public GenerationCapabilities Capabilities { get; }

    public Task<AudioGenerationResponse> GenerateAsync(
        AudioGenerationRequest request,
        CancellationToken cancellationToken = default) =>
        _pipeline.InvokeAsync(request, cancellationToken);
}
