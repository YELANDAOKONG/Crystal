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

public sealed class CombinedGenerationClientDecoratorTests
{
    [Fact]
    public async Task ImageWrapperKeepsBothInterfacesAndTheirCapabilityProfiles()
    {
        var immediateCapabilities = Capabilities(ContentModality.Image);
        var streamCapabilities = Capabilities(ContentModality.Image, true);
        var response = new ImageGenerationResponse([]);
        var terminal = new GenerationStreamCompleted<ImageGenerationResponse>(response);
        var source = new ImageClient(immediateCapabilities, streamCapabilities,
            response, terminal);
        var request = new ImageGenerationRequest([]);
        using var cancellation = new CancellationTokenSource();
        var completeCalls = 0;
        var streamEvents = 0;
        var wrapped = Clients.ForImageGeneration(source,
            [Count<ImageGenerationRequest, ImageGenerationResponse>(
                () => completeCalls++)],
            [Observe<ImageGenerationRequest, ImageGenerationResponse>(
                () => streamEvents++)]);
        var streaming = Assert.IsAssignableFrom<IStreamingImageGenerationClient>(
            wrapped);

        var actualResponse = await wrapped.GenerateAsync(request, cancellation.Token);
        var actualEvents = await CollectAsync(streaming.StreamAsync(
            request, cancellation.Token));

        Assert.Same(immediateCapabilities, wrapped.Capabilities);
        Assert.Same(streamCapabilities, streaming.Capabilities);
        Assert.Same(response, actualResponse);
        Assert.Same(terminal, Assert.Single(actualEvents));
        Assert.Same(request, source.CompleteRequest);
        Assert.Same(request, source.StreamRequest);
        Assert.Equal(cancellation.Token, source.CompleteToken);
        Assert.Equal(cancellation.Token, source.StreamToken);
        Assert.Equal(1, completeCalls);
        Assert.Equal(1, streamEvents);
    }

    [Fact]
    public async Task AudioWrapperKeepsBothInterfacesAndTheirCapabilityProfiles()
    {
        var immediateCapabilities = Capabilities(ContentModality.Audio);
        var streamCapabilities = Capabilities(ContentModality.Audio, true);
        var response = new AudioGenerationResponse([]);
        var terminal = new GenerationStreamCompleted<AudioGenerationResponse>(response);
        var source = new AudioClient(immediateCapabilities, streamCapabilities,
            response, terminal);
        var request = new AudioGenerationRequest([]);
        using var cancellation = new CancellationTokenSource();
        var wrapped = Clients.ForAudioGeneration(source, [], []);
        var streaming = Assert.IsAssignableFrom<IStreamingAudioGenerationClient>(
            wrapped);

        Assert.Same(response, await wrapped.GenerateAsync(request, cancellation.Token));
        Assert.Same(terminal, Assert.Single(await CollectAsync(
            streaming.StreamAsync(request, cancellation.Token))));
        Assert.Same(immediateCapabilities, wrapped.Capabilities);
        Assert.Same(streamCapabilities, streaming.Capabilities);
        Assert.Same(request, source.CompleteRequest);
        Assert.Same(request, source.StreamRequest);
        Assert.Equal(cancellation.Token, source.CompleteToken);
        Assert.Equal(cancellation.Token, source.StreamToken);
    }

    [Fact]
    public async Task VideoWrapperKeepsBothInterfacesAndTheirCapabilityProfiles()
    {
        var immediateCapabilities = Capabilities(ContentModality.Video);
        var streamCapabilities = Capabilities(ContentModality.Video, true);
        var response = new VideoGenerationResponse([]);
        var terminal = new GenerationStreamCompleted<VideoGenerationResponse>(response);
        var source = new VideoClient(immediateCapabilities, streamCapabilities,
            response, terminal);
        var request = new VideoGenerationRequest([]);
        using var cancellation = new CancellationTokenSource();
        var wrapped = Clients.ForVideoGeneration(source, [], []);
        var streaming = Assert.IsAssignableFrom<IStreamingVideoGenerationClient>(
            wrapped);

        Assert.Same(response, await wrapped.GenerateAsync(request, cancellation.Token));
        Assert.Same(terminal, Assert.Single(await CollectAsync(
            streaming.StreamAsync(request, cancellation.Token))));
        Assert.Same(immediateCapabilities, wrapped.Capabilities);
        Assert.Same(streamCapabilities, streaming.Capabilities);
        Assert.Same(request, source.CompleteRequest);
        Assert.Same(request, source.StreamRequest);
        Assert.Equal(cancellation.Token, source.CompleteToken);
        Assert.Equal(cancellation.Token, source.StreamToken);
    }

    [Fact]
    public void ImageWrapperRejectsStreamMiddlewareWithoutStreamSupport()
    {
        var source = new ImmediateImageClient(Capabilities(ContentModality.Image));
        StreamingMiddleware<ImageGenerationRequest,
            GenerationStreamEvent<ImageGenerationResponse>> middleware =
            next => next;

        var failure = Assert.Throws<ArgumentException>(() =>
            Clients.ForImageGeneration(source, [], [middleware]));

        Assert.Equal("streamingMiddleware", failure.ParamName);
        Assert.IsNotAssignableFrom<IStreamingImageGenerationClient>(
            Clients.ForImageGeneration(source, []));
    }

    private static GenerationCapabilities Capabilities(
        ContentModality modality,
        bool supportsReasoningOptions = false) =>
        new([], [new MultimodalContentCapability(
            modality, [MediaSourceKind.Inline])],
            supportsReasoningOptions);

    private static AsyncMiddleware<TRequest, TResponse> Count<TRequest, TResponse>(
        Action observed) =>
        next => async (request, cancellationToken) =>
        {
            observed();
            return await next(request, cancellationToken).ConfigureAwait(false);
        };

    private static StreamingMiddleware<TRequest, GenerationStreamEvent<TResponse>>
        Observe<TRequest, TResponse>(Action observed)
        where TResponse : class =>
        next => (request, cancellationToken) =>
            ObserveAsync(next(request, cancellationToken), observed);

    private static async IAsyncEnumerable<GenerationStreamEvent<TResponse>>
        ObserveAsync<TResponse>(
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
        GenerationCapabilities immediateCapabilities,
        GenerationCapabilities streamCapabilities,
        ImageGenerationResponse response,
        GenerationStreamEvent<ImageGenerationResponse> streamEvent) :
        IImageGenerationClient,
        IStreamingImageGenerationClient
    {
        public GenerationCapabilities Capabilities => immediateCapabilities;

        GenerationCapabilities IStreamingImageGenerationClient.Capabilities =>
            streamCapabilities;

        public ImageGenerationRequest? CompleteRequest { get; private set; }

        public ImageGenerationRequest? StreamRequest { get; private set; }

        public CancellationToken CompleteToken { get; private set; }

        public CancellationToken StreamToken { get; private set; }

        public Task<ImageGenerationResponse> GenerateAsync(
            ImageGenerationRequest request,
            CancellationToken cancellationToken = default)
        {
            CompleteRequest = request;
            CompleteToken = cancellationToken;
            return Task.FromResult(response);
        }

        public IAsyncEnumerable<GenerationStreamEvent<ImageGenerationResponse>>
            StreamAsync(
                ImageGenerationRequest request,
                CancellationToken cancellationToken = default)
        {
            StreamRequest = request;
            StreamToken = cancellationToken;
            return Events(streamEvent);
        }
    }

    private sealed class AudioClient(
        GenerationCapabilities immediateCapabilities,
        GenerationCapabilities streamCapabilities,
        AudioGenerationResponse response,
        GenerationStreamEvent<AudioGenerationResponse> streamEvent) :
        IAudioGenerationClient,
        IStreamingAudioGenerationClient
    {
        public GenerationCapabilities Capabilities => immediateCapabilities;

        GenerationCapabilities IStreamingAudioGenerationClient.Capabilities =>
            streamCapabilities;

        public AudioGenerationRequest? CompleteRequest { get; private set; }

        public AudioGenerationRequest? StreamRequest { get; private set; }

        public CancellationToken CompleteToken { get; private set; }

        public CancellationToken StreamToken { get; private set; }

        public Task<AudioGenerationResponse> GenerateAsync(
            AudioGenerationRequest request,
            CancellationToken cancellationToken = default)
        {
            CompleteRequest = request;
            CompleteToken = cancellationToken;
            return Task.FromResult(response);
        }

        public IAsyncEnumerable<GenerationStreamEvent<AudioGenerationResponse>>
            StreamAsync(
                AudioGenerationRequest request,
                CancellationToken cancellationToken = default)
        {
            StreamRequest = request;
            StreamToken = cancellationToken;
            return Events(streamEvent);
        }
    }

    private sealed class VideoClient(
        GenerationCapabilities immediateCapabilities,
        GenerationCapabilities streamCapabilities,
        VideoGenerationResponse response,
        GenerationStreamEvent<VideoGenerationResponse> streamEvent) :
        IVideoGenerationClient,
        IStreamingVideoGenerationClient
    {
        public GenerationCapabilities Capabilities => immediateCapabilities;

        GenerationCapabilities IStreamingVideoGenerationClient.Capabilities =>
            streamCapabilities;

        public VideoGenerationRequest? CompleteRequest { get; private set; }

        public VideoGenerationRequest? StreamRequest { get; private set; }

        public CancellationToken CompleteToken { get; private set; }

        public CancellationToken StreamToken { get; private set; }

        public Task<VideoGenerationResponse> GenerateAsync(
            VideoGenerationRequest request,
            CancellationToken cancellationToken = default)
        {
            CompleteRequest = request;
            CompleteToken = cancellationToken;
            return Task.FromResult(response);
        }

        public IAsyncEnumerable<GenerationStreamEvent<VideoGenerationResponse>>
            StreamAsync(
                VideoGenerationRequest request,
                CancellationToken cancellationToken = default)
        {
            StreamRequest = request;
            StreamToken = cancellationToken;
            return Events(streamEvent);
        }
    }

    private sealed class ImmediateImageClient(GenerationCapabilities capabilities) :
        IImageGenerationClient
    {
        public GenerationCapabilities Capabilities { get; } = capabilities;

        public Task<ImageGenerationResponse> GenerateAsync(
            ImageGenerationRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new ImageGenerationResponse([]));
    }
}
