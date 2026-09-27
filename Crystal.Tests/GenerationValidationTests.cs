using Crystal.Decorators;
using Crystal.Generation;
using Crystal.Generation.Batches;
using Crystal.Generation.Images;
using Crystal.Generation.Operations;
using Crystal.Generation.Streaming;
using Crystal.Generation.Video;
using Crystal.Media;
using Crystal.Multimodal;
using Crystal.Pipelines;

namespace Crystal.Tests;

public sealed class GenerationValidationTests
{
    private static readonly MediaMimeType Png = new("image/png");

    [Fact]
    public async Task DeclaredTextAndMediaShapesForwardExactRequestAndToken()
    {
        var capabilities = Capabilities();
        var request = new ImageGenerationRequest(
        [
            new GenerationTextInput("caller instruction"),
            ImageInput(MediaSourceKind.Inline)
        ]);
        var response = new ImageGenerationResponse([]);
        using var cancellation = new CancellationTokenSource();
        var pipeline = new AsyncPipeline<ImageGenerationRequest,
            ImageGenerationResponse>(
            (actualRequest, token) =>
            {
                Assert.Same(request, actualRequest);
                Assert.Equal(cancellation.Token, token);
                return Task.FromResult(response);
            },
            [GenerationValidation.RequireDeclaredInputShapes<
                ImageGenerationRequest, ImageGenerationResponse>(
                capabilities, static value => value.Inputs)]);

        Assert.Same(response,
            await pipeline.InvokeAsync(request, cancellation.Token));
    }

    [Fact]
    public async Task UndeclaredPurposeAndMediaSourceFailBeforeClientCall()
    {
        var invoked = false;
        var pipeline = new AsyncPipeline<ImageGenerationRequest,
            ImageGenerationResponse>(
            (_, _) =>
            {
                invoked = true;
                return Task.FromResult(new ImageGenerationResponse([]));
            },
            [GenerationValidation.RequireDeclaredInputShapes<
                ImageGenerationRequest, ImageGenerationResponse>(
                Capabilities(), static value => value.Inputs)]);

        var wrongPurpose = new ImageGenerationRequest(
            [ImageInput(MediaSourceKind.Inline, GenerationInputPurpose.Source)]);
        var wrongSource = new ImageGenerationRequest(
            [ImageInput(MediaSourceKind.Uri)]);

        foreach (var request in new[] { wrongPurpose, wrongSource })
        {
            var failure = await Assert.ThrowsAsync<ArgumentException>(() =>
                pipeline.InvokeAsync(request));
            Assert.Equal("request", failure.ParamName);
            Assert.DoesNotContain("caller", failure.Message);
            Assert.DoesNotContain("private", failure.Message);
        }

        Assert.False(invoked);
    }

    [Fact]
    public async Task BatchSelectorChecksEverySubmittedInputBeforeSubmission()
    {
        var request = new GenerationBatchRequest<ImageGenerationRequest>(
        [
            new ImageGenerationRequest([new GenerationTextInput("first")]),
            new ImageGenerationRequest([ImageInput(MediaSourceKind.Uri)])
        ]);
        var invoked = false;
        var pipeline = new AsyncPipeline<GenerationBatchRequest<ImageGenerationRequest>,
            GenerationOperationSnapshot<GenerationBatchResponse<ImageGenerationResponse>>>(
            (_, _) =>
            {
                invoked = true;
                throw new InvalidOperationException("Submission should not start.");
            },
            [GenerationValidation.RequireDeclaredInputShapes<
                GenerationBatchRequest<ImageGenerationRequest>,
                GenerationOperationSnapshot<GenerationBatchResponse<
                    ImageGenerationResponse>>>(
                Capabilities(),
                static batch => batch.Requests.SelectMany(
                    static item => item.Inputs))]);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            pipeline.InvokeAsync(request));
        Assert.False(invoked);
    }

    [Fact]
    public async Task StreamPreflightForwardsValidEventsAndRejectsInvalidRequest()
    {
        var request = new ImageGenerationRequest(
            [ImageInput(MediaSourceKind.Inline)]);
        var terminal = new GenerationStreamCompleted<ImageGenerationResponse>(
            new ImageGenerationResponse([]));
        using var cancellation = new CancellationTokenSource();
        var calls = 0;
        var pipeline = new StreamingPipeline<ImageGenerationRequest,
            GenerationStreamEvent<ImageGenerationResponse>>(
            (actualRequest, token) =>
            {
                calls++;
                Assert.Same(request, actualRequest);
                Assert.Equal(cancellation.Token, token);
                return Events(terminal);
            },
            [GenerationValidation.RequireDeclaredStreamInputShapes<
                ImageGenerationRequest,
                GenerationStreamEvent<ImageGenerationResponse>>(
                Capabilities(), static value => value.Inputs)]);

        var actual = await CollectAsync(
            pipeline.StreamAsync(request, cancellation.Token));
        Assert.Same(terminal, Assert.Single(actual));

        var wrongSource = new ImageGenerationRequest(
            [ImageInput(MediaSourceKind.Uri)]);
        await Assert.ThrowsAsync<ArgumentException>(() =>
            CollectAsync(pipeline.StreamAsync(
                wrongSource, cancellation.Token)));
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task VideoGenerationAcceptsDeclaredAudioAndVideoConditioning()
    {
        var capabilities = new GenerationCapabilities(
        [
            new GenerationInputCapability(
                new MultimodalContentCapability(
                    ContentModality.Audio, [MediaSourceKind.Inline]),
                GenerationInputPurpose.Reference),
            new GenerationInputCapability(
                new MultimodalContentCapability(
                    ContentModality.Video, [MediaSourceKind.Inline]),
                GenerationInputPurpose.Source)
        ],
        [
            new MultimodalContentCapability(
                ContentModality.Video, [MediaSourceKind.Inline])
        ]);
        var request = new VideoGenerationRequest(
        [
            new GenerationAudioInput(
                new AudioMedia(
                    new InlineMediaSource(new byte[] { 1 }),
                    new MediaMimeType("audio/wav")),
                GenerationInputPurpose.Reference),
            new GenerationVideoInput(
                new VideoMedia(
                    new InlineMediaSource(new byte[] { 2 }),
                    new MediaMimeType("video/mp4")),
                GenerationInputPurpose.Source)
        ]);
        var response = new VideoGenerationResponse([]);
        var pipeline = new AsyncPipeline<VideoGenerationRequest,
            VideoGenerationResponse>(
            (actualRequest, _) =>
            {
                Assert.Same(request, actualRequest);
                return Task.FromResult(response);
            },
            [GenerationValidation.RequireDeclaredInputShapes<
                VideoGenerationRequest, VideoGenerationResponse>(
                capabilities, static value => value.Inputs)]);

        Assert.Same(response, await pipeline.InvokeAsync(request));
    }

    private static GenerationCapabilities Capabilities() =>
        new(
        [
            new GenerationInputCapability(
                new MultimodalContentCapability(ContentModality.Text),
                GenerationInputPurpose.Instruction),
            new GenerationInputCapability(
                new MultimodalContentCapability(
                    ContentModality.Image, [MediaSourceKind.Inline]),
                GenerationInputPurpose.Reference)
        ],
        [
            new MultimodalContentCapability(
                ContentModality.Image, [MediaSourceKind.Inline])
        ]);

    private static GenerationImageInput ImageInput(
        MediaSourceKind sourceKind,
        GenerationInputPurpose? purpose = null)
    {
        MediaSource source = sourceKind == MediaSourceKind.Inline
            ? new InlineMediaSource(new byte[] { 1 })
            : new UriMediaSource(new Uri("https://example.invalid/private"));
        return new GenerationImageInput(
            new ImageMedia(source, Png),
            purpose ?? GenerationInputPurpose.Reference);
    }

    private static async IAsyncEnumerable<
        GenerationStreamEvent<ImageGenerationResponse>> Events(
            GenerationStreamEvent<ImageGenerationResponse> streamEvent)
    {
        await Task.Yield();
        yield return streamEvent;
    }

    private static async Task<List<
        GenerationStreamEvent<ImageGenerationResponse>>> CollectAsync(
            IAsyncEnumerable<GenerationStreamEvent<ImageGenerationResponse>> events)
    {
        var collected = new List<GenerationStreamEvent<ImageGenerationResponse>>();
        await foreach (var streamEvent in events)
        {
            collected.Add(streamEvent);
        }

        return collected;
    }
}
