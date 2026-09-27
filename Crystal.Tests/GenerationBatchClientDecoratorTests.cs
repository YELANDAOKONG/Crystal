using Crystal.Decorators;
using Crystal.Generation;
using Crystal.Generation.Audio;
using Crystal.Generation.Batches;
using Crystal.Generation.Images;
using Crystal.Generation.Operations;
using Crystal.Generation.Video;
using Crystal.Media;
using Crystal.Multimodal;

namespace Crystal.Tests;

public sealed class GenerationBatchClientDecoratorTests
{
    [Fact]
    public async Task ImageWrapperComposesCardinalityValidationAndPreservesValues()
    {
        var capabilities = Capabilities(ContentModality.Image);
        var request = new GenerationBatchRequest<ImageGenerationRequest>(
            [new ImageGenerationRequest([])]);
        var ticket = Ticket();
        var response = new GenerationBatchResponse<ImageGenerationResponse>(
            1,
            [new GenerationBatchItemResult<ImageGenerationResponse>(
                GenerationBatchItemStatus.Completed,
                new ImageGenerationResponse([]))]);
        var completed = new GenerationOperationSnapshot<
            GenerationBatchResponse<ImageGenerationResponse>>(
                ticket, GenerationOperationStatus.Completed, response);
        var source = new ImageClient(capabilities, completed);
        using var cancellation = new CancellationTokenSource();
        var wrapped = Clients.ForImageGenerationBatch(source,
            [GenerationBatchValidation.RequireSubmittedCardinality<
                ImageGenerationRequest, ImageGenerationResponse>()],
            [GenerationBatchValidation.RequirePolledCardinality<
                ImageGenerationResponse>(request.Requests.Count)]);

        var started = await wrapped.StartBatchAsync(request, cancellation.Token);
        var polled = await wrapped.PollBatchAsync(ticket, cancellation.Token);

        Assert.Same(capabilities, wrapped.Capabilities);
        Assert.Same(completed, started);
        Assert.Same(completed, polled);
        Assert.Same(request, source.Request);
        Assert.Same(ticket, source.PollTicket);
        Assert.Equal(cancellation.Token, source.StartToken);
        Assert.Equal(cancellation.Token, source.PollToken);
    }

    [Fact]
    public async Task AudioWrapperPreservesSubmissionAndPoll()
    {
        var capabilities = Capabilities(ContentModality.Audio);
        var request = new GenerationBatchRequest<AudioGenerationRequest>(
            [new AudioGenerationRequest([])]);
        var ticket = Ticket();
        var pending = new GenerationOperationSnapshot<
            GenerationBatchResponse<AudioGenerationResponse>>(
                ticket, GenerationOperationStatus.Running);
        var source = new AudioClient(capabilities, pending);
        using var cancellation = new CancellationTokenSource();
        var wrapped = Clients.ForAudioGenerationBatch(source, [], []);

        Assert.Same(pending,
            await wrapped.StartBatchAsync(request, cancellation.Token));
        Assert.Same(pending,
            await wrapped.PollBatchAsync(ticket, cancellation.Token));
        Assert.Same(capabilities, wrapped.Capabilities);
        Assert.Same(request, source.Request);
        Assert.Same(ticket, source.PollTicket);
        Assert.Equal(cancellation.Token, source.StartToken);
        Assert.Equal(cancellation.Token, source.PollToken);
    }

    [Fact]
    public async Task VideoWrapperPreservesSubmissionAndPoll()
    {
        var capabilities = Capabilities(ContentModality.Video);
        var request = new GenerationBatchRequest<VideoGenerationRequest>(
            [new VideoGenerationRequest([])]);
        var ticket = Ticket();
        var pending = new GenerationOperationSnapshot<
            GenerationBatchResponse<VideoGenerationResponse>>(
                ticket, GenerationOperationStatus.Running);
        var source = new VideoClient(capabilities, pending);
        using var cancellation = new CancellationTokenSource();
        var wrapped = Clients.ForVideoGenerationBatch(source, [], []);

        Assert.Same(pending,
            await wrapped.StartBatchAsync(request, cancellation.Token));
        Assert.Same(pending,
            await wrapped.PollBatchAsync(ticket, cancellation.Token));
        Assert.Same(capabilities, wrapped.Capabilities);
        Assert.Same(request, source.Request);
        Assert.Same(ticket, source.PollTicket);
        Assert.Equal(cancellation.Token, source.StartToken);
        Assert.Equal(cancellation.Token, source.PollToken);
    }

    private static GenerationCapabilities Capabilities(ContentModality modality) =>
        new([], [new MultimodalContentCapability(
            modality, [MediaSourceKind.Inline])]);

    private static GenerationOperationTicket Ticket() =>
        new("test", new byte[] { 1 });

    private sealed class ImageClient(
        GenerationCapabilities capabilities,
        GenerationOperationSnapshot<GenerationBatchResponse<ImageGenerationResponse>>
            snapshot) : IImageGenerationBatchClient
    {
        public GenerationCapabilities Capabilities { get; } = capabilities;

        public GenerationBatchRequest<ImageGenerationRequest>? Request { get; private set; }

        public GenerationOperationTicket? PollTicket { get; private set; }

        public CancellationToken StartToken { get; private set; }

        public CancellationToken PollToken { get; private set; }

        public Task<GenerationOperationSnapshot<
            GenerationBatchResponse<ImageGenerationResponse>>> StartBatchAsync(
                GenerationBatchRequest<ImageGenerationRequest> request,
                CancellationToken cancellationToken = default)
        {
            Request = request;
            StartToken = cancellationToken;
            return Task.FromResult(snapshot);
        }

        public Task<GenerationOperationSnapshot<
            GenerationBatchResponse<ImageGenerationResponse>>> PollBatchAsync(
                GenerationOperationTicket ticket,
                CancellationToken cancellationToken = default)
        {
            PollTicket = ticket;
            PollToken = cancellationToken;
            return Task.FromResult(snapshot);
        }
    }

    private sealed class AudioClient(
        GenerationCapabilities capabilities,
        GenerationOperationSnapshot<GenerationBatchResponse<AudioGenerationResponse>>
            snapshot) : IAudioGenerationBatchClient
    {
        public GenerationCapabilities Capabilities { get; } = capabilities;

        public GenerationBatchRequest<AudioGenerationRequest>? Request { get; private set; }

        public GenerationOperationTicket? PollTicket { get; private set; }

        public CancellationToken StartToken { get; private set; }

        public CancellationToken PollToken { get; private set; }

        public Task<GenerationOperationSnapshot<
            GenerationBatchResponse<AudioGenerationResponse>>> StartBatchAsync(
                GenerationBatchRequest<AudioGenerationRequest> request,
                CancellationToken cancellationToken = default)
        {
            Request = request;
            StartToken = cancellationToken;
            return Task.FromResult(snapshot);
        }

        public Task<GenerationOperationSnapshot<
            GenerationBatchResponse<AudioGenerationResponse>>> PollBatchAsync(
                GenerationOperationTicket ticket,
                CancellationToken cancellationToken = default)
        {
            PollTicket = ticket;
            PollToken = cancellationToken;
            return Task.FromResult(snapshot);
        }
    }

    private sealed class VideoClient(
        GenerationCapabilities capabilities,
        GenerationOperationSnapshot<GenerationBatchResponse<VideoGenerationResponse>>
            snapshot) : IVideoGenerationBatchClient
    {
        public GenerationCapabilities Capabilities { get; } = capabilities;

        public GenerationBatchRequest<VideoGenerationRequest>? Request { get; private set; }

        public GenerationOperationTicket? PollTicket { get; private set; }

        public CancellationToken StartToken { get; private set; }

        public CancellationToken PollToken { get; private set; }

        public Task<GenerationOperationSnapshot<
            GenerationBatchResponse<VideoGenerationResponse>>> StartBatchAsync(
                GenerationBatchRequest<VideoGenerationRequest> request,
                CancellationToken cancellationToken = default)
        {
            Request = request;
            StartToken = cancellationToken;
            return Task.FromResult(snapshot);
        }

        public Task<GenerationOperationSnapshot<
            GenerationBatchResponse<VideoGenerationResponse>>> PollBatchAsync(
                GenerationOperationTicket ticket,
                CancellationToken cancellationToken = default)
        {
            PollTicket = ticket;
            PollToken = cancellationToken;
            return Task.FromResult(snapshot);
        }
    }
}
