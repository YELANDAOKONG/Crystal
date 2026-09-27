using Crystal.Decorators;
using Crystal.Generation;
using Crystal.Generation.Audio;
using Crystal.Generation.Images;
using Crystal.Generation.Operations;
using Crystal.Generation.Video;
using Crystal.Media;
using Crystal.Multimodal;
using Crystal.Pipelines;

namespace Crystal.Tests;

public sealed class GenerationOperationClientDecoratorTests
{
    [Fact]
    public async Task ImageWrapperPreservesBothOperationPaths()
    {
        var capabilities = Capabilities(ContentModality.Image);
        var source = new ImageClient(capabilities);
        var request = new ImageGenerationRequest([]);
        using var cancellation = new CancellationTokenSource();
        var starts = 0;
        var polls = 0;
        var wrapped = Clients.ForImageGenerationOperation(source,
            [Count<ImageGenerationRequest, ImageGenerationResponse>(
                () => starts++)],
            [Count<GenerationOperationTicket, ImageGenerationResponse>(
                () => polls++)]);

        var started = await wrapped.StartAsync(request, cancellation.Token);
        var polled = await wrapped.PollAsync(started.Ticket, cancellation.Token);

        Assert.Same(capabilities, wrapped.Capabilities);
        Assert.Same(source.StartSnapshot, started);
        Assert.Same(source.PollSnapshot, polled);
        Assert.Same(request, source.Request);
        Assert.Same(started.Ticket, source.PollTicket);
        Assert.Equal(cancellation.Token, source.StartToken);
        Assert.Equal(cancellation.Token, source.PollToken);
        Assert.Equal(1, starts);
        Assert.Equal(1, polls);
    }

    [Fact]
    public async Task AudioWrapperPreservesBothOperationPaths()
    {
        var capabilities = Capabilities(ContentModality.Audio);
        var source = new AudioClient(capabilities);
        var request = new AudioGenerationRequest([]);
        using var cancellation = new CancellationTokenSource();
        var starts = 0;
        var polls = 0;
        var wrapped = Clients.ForAudioGenerationOperation(source,
            [Count<AudioGenerationRequest, AudioGenerationResponse>(
                () => starts++)],
            [Count<GenerationOperationTicket, AudioGenerationResponse>(
                () => polls++)]);

        var started = await wrapped.StartAsync(request, cancellation.Token);
        var polled = await wrapped.PollAsync(started.Ticket, cancellation.Token);

        Assert.Same(capabilities, wrapped.Capabilities);
        Assert.Same(source.StartSnapshot, started);
        Assert.Same(source.PollSnapshot, polled);
        Assert.Same(request, source.Request);
        Assert.Same(started.Ticket, source.PollTicket);
        Assert.Equal(cancellation.Token, source.StartToken);
        Assert.Equal(cancellation.Token, source.PollToken);
        Assert.Equal(1, starts);
        Assert.Equal(1, polls);
    }

    [Fact]
    public async Task VideoWrapperPreservesBothOperationPaths()
    {
        var capabilities = Capabilities(ContentModality.Video);
        var source = new VideoClient(capabilities);
        var request = new VideoGenerationRequest([]);
        using var cancellation = new CancellationTokenSource();
        var starts = 0;
        var polls = 0;
        var wrapped = Clients.ForVideoGenerationOperation(source,
            [Count<VideoGenerationRequest, VideoGenerationResponse>(
                () => starts++)],
            [Count<GenerationOperationTicket, VideoGenerationResponse>(
                () => polls++)]);

        var started = await wrapped.StartAsync(request, cancellation.Token);
        var polled = await wrapped.PollAsync(started.Ticket, cancellation.Token);

        Assert.Same(capabilities, wrapped.Capabilities);
        Assert.Same(source.StartSnapshot, started);
        Assert.Same(source.PollSnapshot, polled);
        Assert.Same(request, source.Request);
        Assert.Same(started.Ticket, source.PollTicket);
        Assert.Equal(cancellation.Token, source.StartToken);
        Assert.Equal(cancellation.Token, source.PollToken);
        Assert.Equal(1, starts);
        Assert.Equal(1, polls);
    }

    private static GenerationCapabilities Capabilities(ContentModality modality) =>
        new([], [new MultimodalContentCapability(
            modality, [MediaSourceKind.Inline])]);

    private static AsyncMiddleware<TRequest,
        GenerationOperationSnapshot<TResponse>> Count<TRequest, TResponse>(
            Action observed)
        where TResponse : class =>
        next => async (request, cancellationToken) =>
        {
            observed();
            return await next(request, cancellationToken).ConfigureAwait(false);
        };

    private sealed class ImageClient(GenerationCapabilities capabilities) :
        IImageGenerationOperationClient
    {
        public GenerationCapabilities Capabilities { get; } = capabilities;

        public GenerationOperationSnapshot<ImageGenerationResponse> StartSnapshot { get; } =
            new(Ticket(), GenerationOperationStatus.Running);

        public GenerationOperationSnapshot<ImageGenerationResponse> PollSnapshot { get; } =
            new(Ticket(), GenerationOperationStatus.Completed,
                new ImageGenerationResponse([]));

        public ImageGenerationRequest? Request { get; private set; }

        public GenerationOperationTicket? PollTicket { get; private set; }

        public CancellationToken StartToken { get; private set; }

        public CancellationToken PollToken { get; private set; }

        public Task<GenerationOperationSnapshot<ImageGenerationResponse>> StartAsync(
            ImageGenerationRequest request,
            CancellationToken cancellationToken = default)
        {
            Request = request;
            StartToken = cancellationToken;
            return Task.FromResult(StartSnapshot);
        }

        public Task<GenerationOperationSnapshot<ImageGenerationResponse>> PollAsync(
            GenerationOperationTicket ticket,
            CancellationToken cancellationToken = default)
        {
            PollTicket = ticket;
            PollToken = cancellationToken;
            return Task.FromResult(PollSnapshot);
        }
    }

    private sealed class AudioClient(GenerationCapabilities capabilities) :
        IAudioGenerationOperationClient
    {
        public GenerationCapabilities Capabilities { get; } = capabilities;

        public GenerationOperationSnapshot<AudioGenerationResponse> StartSnapshot { get; } =
            new(Ticket(), GenerationOperationStatus.Running);

        public GenerationOperationSnapshot<AudioGenerationResponse> PollSnapshot { get; } =
            new(Ticket(), GenerationOperationStatus.Completed,
                new AudioGenerationResponse([]));

        public AudioGenerationRequest? Request { get; private set; }

        public GenerationOperationTicket? PollTicket { get; private set; }

        public CancellationToken StartToken { get; private set; }

        public CancellationToken PollToken { get; private set; }

        public Task<GenerationOperationSnapshot<AudioGenerationResponse>> StartAsync(
            AudioGenerationRequest request,
            CancellationToken cancellationToken = default)
        {
            Request = request;
            StartToken = cancellationToken;
            return Task.FromResult(StartSnapshot);
        }

        public Task<GenerationOperationSnapshot<AudioGenerationResponse>> PollAsync(
            GenerationOperationTicket ticket,
            CancellationToken cancellationToken = default)
        {
            PollTicket = ticket;
            PollToken = cancellationToken;
            return Task.FromResult(PollSnapshot);
        }
    }

    private sealed class VideoClient(GenerationCapabilities capabilities) :
        IVideoGenerationOperationClient
    {
        public GenerationCapabilities Capabilities { get; } = capabilities;

        public GenerationOperationSnapshot<VideoGenerationResponse> StartSnapshot { get; } =
            new(Ticket(), GenerationOperationStatus.Running);

        public GenerationOperationSnapshot<VideoGenerationResponse> PollSnapshot { get; } =
            new(Ticket(), GenerationOperationStatus.Completed,
                new VideoGenerationResponse([]));

        public VideoGenerationRequest? Request { get; private set; }

        public GenerationOperationTicket? PollTicket { get; private set; }

        public CancellationToken StartToken { get; private set; }

        public CancellationToken PollToken { get; private set; }

        public Task<GenerationOperationSnapshot<VideoGenerationResponse>> StartAsync(
            VideoGenerationRequest request,
            CancellationToken cancellationToken = default)
        {
            Request = request;
            StartToken = cancellationToken;
            return Task.FromResult(StartSnapshot);
        }

        public Task<GenerationOperationSnapshot<VideoGenerationResponse>> PollAsync(
            GenerationOperationTicket ticket,
            CancellationToken cancellationToken = default)
        {
            PollTicket = ticket;
            PollToken = cancellationToken;
            return Task.FromResult(PollSnapshot);
        }
    }

    private static GenerationOperationTicket Ticket() =>
        new("test", new byte[] { 1 });
}
