using Crystal.Generation;
using Crystal.Generation.Audio;
using Crystal.Generation.Streaming;
using Crystal.Pipelines;

namespace Crystal.Decorators.Generation;

internal sealed class CombinedAudioGenerationClientAdapter :
    IAudioGenerationClient,
    IStreamingAudioGenerationClient
{
    private readonly AsyncPipeline<AudioGenerationRequest, AudioGenerationResponse> _generate;
    private readonly StreamingPipeline<AudioGenerationRequest,
        GenerationStreamEvent<AudioGenerationResponse>> _stream;
    private readonly GenerationCapabilities _streamCapabilities;

    public CombinedAudioGenerationClientAdapter(
        IAudioGenerationClient client,
        IStreamingAudioGenerationClient streamingClient,
        IEnumerable<AsyncMiddleware<AudioGenerationRequest,
            AudioGenerationResponse>> middleware,
        IEnumerable<StreamingMiddleware<AudioGenerationRequest,
            GenerationStreamEvent<AudioGenerationResponse>>> streamingMiddleware)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(streamingClient);
        Capabilities = client.Capabilities;
        _streamCapabilities = streamingClient.Capabilities;
        _generate = new(client.GenerateAsync, middleware);
        _stream = new(streamingClient.StreamAsync, streamingMiddleware);
    }

    public GenerationCapabilities Capabilities { get; }

    GenerationCapabilities IStreamingAudioGenerationClient.Capabilities =>
        _streamCapabilities;

    public Task<AudioGenerationResponse> GenerateAsync(
        AudioGenerationRequest request,
        CancellationToken cancellationToken = default) =>
        _generate.InvokeAsync(request, cancellationToken);

    public IAsyncEnumerable<GenerationStreamEvent<AudioGenerationResponse>>
        StreamAsync(
            AudioGenerationRequest request,
            CancellationToken cancellationToken = default) =>
        _stream.StreamAsync(request, cancellationToken);
}
