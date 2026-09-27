using Crystal.Generation;
using Crystal.Generation.Audio;
using Crystal.Generation.Streaming;
using Crystal.Pipelines;

namespace Crystal.Decorators.Generation;

internal sealed class StreamingAudioGenerationClientAdapter : IStreamingAudioGenerationClient
{
    private readonly StreamingPipeline<AudioGenerationRequest,
        GenerationStreamEvent<AudioGenerationResponse>> _pipeline;

    public StreamingAudioGenerationClientAdapter(
        IStreamingAudioGenerationClient client,
        IEnumerable<StreamingMiddleware<AudioGenerationRequest,
            GenerationStreamEvent<AudioGenerationResponse>>> middleware)
    {
        ArgumentNullException.ThrowIfNull(client);
        Capabilities = client.Capabilities;
        _pipeline = new StreamingPipeline<AudioGenerationRequest,
            GenerationStreamEvent<AudioGenerationResponse>>(
            client.StreamAsync,
            middleware);
    }

    public GenerationCapabilities Capabilities { get; }

    public IAsyncEnumerable<GenerationStreamEvent<AudioGenerationResponse>>
        StreamAsync(
            AudioGenerationRequest request,
            CancellationToken cancellationToken = default) =>
        _pipeline.StreamAsync(request, cancellationToken);
}
