using Crystal.Decorators;
using Crystal.Generation;
using Crystal.Generation.Audio;
using Crystal.Generation.Images;
using Crystal.Generation.Streaming;
using Crystal.Generation.Video;
using Crystal.Media;
using Crystal.Multimodal;
using Crystal.Pipelines;

namespace Crystal.Tests;

public sealed class GenerationStreamingClientDecoratorTests
{
    [Fact]
    public async Task ImageWrapperPreservesCapabilitiesRequestTokenAndEvent()
    {
        var capabilities = Capabilities(ContentModality.Image);
        var request = new ImageGenerationRequest([]);
        var terminal = new GenerationStreamCompleted<ImageGenerationResponse>(
            new ImageGenerationResponse([]));
        var source = new ImageClient(capabilities, terminal);
        using var cancellation = new CancellationTokenSource();
        var observed = 0;
        var wrapped = Clients.ForStreamingImageGeneration(source,
            [Count<ImageGenerationRequest, ImageGenerationResponse>(
                () => observed++),
                GenerationStreamValidation.RequireProtocol<ImageGenerationRequest,
                    ImageGenerationResponse>()]);

        var events = await CollectAsync(wrapped.StreamAsync(
            request, cancellation.Token));

        Assert.Same(capabilities, wrapped.Capabilities);
        Assert.Same(request, source.Request);
        Assert.Equal(cancellation.Token, source.Token);
        Assert.Same(terminal, Assert.Single(events));
        Assert.Equal(1, observed);
    }

    [Fact]
    public async Task AudioWrapperPreservesCapabilitiesRequestTokenAndEvent()
    {
        var capabilities = Capabilities(ContentModality.Audio);
        var request = new AudioGenerationRequest([]);
        var terminal = new GenerationStreamCompleted<AudioGenerationResponse>(
            new AudioGenerationResponse([]));
        var source = new AudioClient(capabilities, terminal);
        using var cancellation = new CancellationTokenSource();
        var observed = 0;
        var wrapped = Clients.ForStreamingAudioGeneration(source,
            [Count<AudioGenerationRequest, AudioGenerationResponse>(
                () => observed++)]);

        var events = await CollectAsync(wrapped.StreamAsync(
            request, cancellation.Token));

        Assert.Same(capabilities, wrapped.Capabilities);
        Assert.Same(request, source.Request);
        Assert.Equal(cancellation.Token, source.Token);
        Assert.Same(terminal, Assert.Single(events));
        Assert.Equal(1, observed);
    }

    [Fact]
    public async Task VideoWrapperPreservesCapabilitiesRequestTokenAndEvent()
    {
        var capabilities = Capabilities(ContentModality.Video);
        var request = new VideoGenerationRequest([]);
        var terminal = new GenerationStreamCompleted<VideoGenerationResponse>(
            new VideoGenerationResponse([]));
        var source = new VideoClient(capabilities, terminal);
        using var cancellation = new CancellationTokenSource();
        var observed = 0;
        var wrapped = Clients.ForStreamingVideoGeneration(source,
            [Count<VideoGenerationRequest, VideoGenerationResponse>(
                () => observed++)]);

        var events = await CollectAsync(wrapped.StreamAsync(
            request, cancellation.Token));

        Assert.Same(capabilities, wrapped.Capabilities);
        Assert.Same(request, source.Request);
        Assert.Equal(cancellation.Token, source.Token);
        Assert.Same(terminal, Assert.Single(events));
        Assert.Equal(1, observed);
    }

    private static GenerationCapabilities Capabilities(ContentModality modality) =>
        new([], [new MultimodalContentCapability(
            modality, [MediaSourceKind.Inline])]);

    private static StreamingMiddleware<TRequest, GenerationStreamEvent<TResponse>>
        Count<TRequest, TResponse>(Action observed)
        where TResponse : class =>
        next => (request, token) => Observe(next(request, token), observed);

    private static async IAsyncEnumerable<GenerationStreamEvent<TResponse>>
        Observe<TResponse>(
            IAsyncEnumerable<GenerationStreamEvent<TResponse>> events,
            Action observed)
        where TResponse : class
    {
        await foreach (var streamEvent in events.ConfigureAwait(false))
        {
            observed();
            yield return streamEvent;
        }
    }

    private static async IAsyncEnumerable<GenerationStreamEvent<TResponse>>
        Events<TResponse>(GenerationStreamEvent<TResponse> streamEvent)
        where TResponse : class
    {
        await Task.Yield();
        yield return streamEvent;
    }

    private static async Task<List<GenerationStreamEvent<TResponse>>>
        CollectAsync<TResponse>(
            IAsyncEnumerable<GenerationStreamEvent<TResponse>> events)
        where TResponse : class
    {
        var collected = new List<GenerationStreamEvent<TResponse>>();
        await foreach (var streamEvent in events)
        {
            collected.Add(streamEvent);
        }

        return collected;
    }

    private sealed class ImageClient(
        GenerationCapabilities capabilities,
        GenerationStreamEvent<ImageGenerationResponse> streamEvent) :
        IStreamingImageGenerationClient
    {
        public GenerationCapabilities Capabilities { get; } = capabilities;

        public ImageGenerationRequest? Request { get; private set; }

        public CancellationToken Token { get; private set; }

        public IAsyncEnumerable<GenerationStreamEvent<ImageGenerationResponse>>
            StreamAsync(
                ImageGenerationRequest request,
                CancellationToken cancellationToken = default)
        {
            Request = request;
            Token = cancellationToken;
            return Events(streamEvent);
        }
    }

    private sealed class AudioClient(
        GenerationCapabilities capabilities,
        GenerationStreamEvent<AudioGenerationResponse> streamEvent) :
        IStreamingAudioGenerationClient
    {
        public GenerationCapabilities Capabilities { get; } = capabilities;

        public AudioGenerationRequest? Request { get; private set; }

        public CancellationToken Token { get; private set; }

        public IAsyncEnumerable<GenerationStreamEvent<AudioGenerationResponse>>
            StreamAsync(
                AudioGenerationRequest request,
                CancellationToken cancellationToken = default)
        {
            Request = request;
            Token = cancellationToken;
            return Events(streamEvent);
        }
    }

    private sealed class VideoClient(
        GenerationCapabilities capabilities,
        GenerationStreamEvent<VideoGenerationResponse> streamEvent) :
        IStreamingVideoGenerationClient
    {
        public GenerationCapabilities Capabilities { get; } = capabilities;

        public VideoGenerationRequest? Request { get; private set; }

        public CancellationToken Token { get; private set; }

        public IAsyncEnumerable<GenerationStreamEvent<VideoGenerationResponse>>
            StreamAsync(
                VideoGenerationRequest request,
                CancellationToken cancellationToken = default)
        {
            Request = request;
            Token = cancellationToken;
            return Events(streamEvent);
        }
    }
}
