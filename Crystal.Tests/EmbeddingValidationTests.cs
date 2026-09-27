using Crystal.Decorators;
using Crystal.Embeddings;
using Crystal.Media;
using Crystal.Multimodal;
using Crystal.Multimodal.Embeddings;
using Crystal.Pipelines;

namespace Crystal.Tests;

public sealed class EmbeddingValidationTests
{
    [Fact]
    public async Task DeclaredShapesPassWithoutOpeningMediaOrChangingRequest()
    {
        var opened = false;
        var replayable = new ReplayableStreamMediaSource(_ =>
        {
            opened = true;
            return ValueTask.FromResult<Stream>(new MemoryStream([1]));
        });
        var request = new MultimodalEmbeddingRequest(
        [
            new MultimodalEmbeddingInput(
            [
                new TextContent("exact"),
                new ImageContent(new ImageMedia(
                    new InlineMediaSource(new byte[] { 1 }),
                    new MediaMimeType("image/png"))),
                new VideoContent(new VideoMedia(
                    replayable,
                    new MediaMimeType("video/mp4")))
            ])
        ]);
        var capabilities = new MultimodalEmbeddingCapabilities(
        [
            new MultimodalContentCapability(ContentModality.Text),
            new MultimodalContentCapability(
                ContentModality.Image,
                [MediaSourceKind.Inline]),
            new MultimodalContentCapability(
                ContentModality.Video,
                [MediaSourceKind.ReplayableStream])
        ]);
        var response = new MultimodalEmbeddingResponse(
            [new EmbeddingVector(new float[] { 1 })]);
        using var cancellation = new CancellationTokenSource();
        var pipeline = new AsyncPipeline<MultimodalEmbeddingRequest,
            MultimodalEmbeddingResponse>(
            (actual, token) =>
            {
                Assert.Same(request, actual);
                Assert.Equal(cancellation.Token, token);
                return Task.FromResult(response);
            },
            [EmbeddingValidation.RequireDeclaredInputShapes(capabilities)]);

        var actual = await pipeline.InvokeAsync(request, cancellation.Token);

        Assert.Same(response, actual);
        Assert.False(opened);
    }

    [Fact]
    public async Task UndeclaredShapesAreRejectedBeforeProviderInvocation()
    {
        var capabilities = new MultimodalEmbeddingCapabilities(
            [
                new MultimodalContentCapability(ContentModality.Text),
                new MultimodalContentCapability(
                    ContentModality.Image,
                    [MediaSourceKind.Inline])
            ]);
        var invoked = false;
        var pipeline = new AsyncPipeline<MultimodalEmbeddingRequest,
            MultimodalEmbeddingResponse>(
            (_, _) =>
            {
                invoked = true;
                throw new InvalidOperationException();
            },
            [EmbeddingValidation.RequireDeclaredInputShapes(capabilities)]);
        var uri = new Uri("https://example.test/private-image");
        var wrongSource = new MultimodalEmbeddingRequest(
            [new MultimodalEmbeddingInput(
                [new ImageContent(new ImageMedia(
                    new UriMediaSource(uri),
                    new MediaMimeType("image/png")))])]);
        var wrongModality = new MultimodalEmbeddingRequest(
            [new MultimodalEmbeddingInput(
                [new AudioContent(new AudioMedia(
                    new InlineMediaSource(new byte[] { 1 }),
                    new MediaMimeType("audio/wav")))])]);

        foreach (var request in new[] { wrongSource, wrongModality })
        {
            var failure = await Assert.ThrowsAsync<ArgumentException>(() =>
                pipeline.InvokeAsync(request));
            Assert.Equal("request", failure.ParamName);
            Assert.DoesNotContain("private-image", failure.Message);
        }

        Assert.False(invoked);
    }

    [Fact]
    public async Task TextValidationReturnsExactValidResponse()
    {
        var request = new EmbeddingRequest(["first", "second"]);
        var response = new EmbeddingResponse(
            [new EmbeddingVector(new float[] { 1 }),
                new EmbeddingVector(new float[] { 2 })]);
        using var cancellation = new CancellationTokenSource();
        AsyncOperation<EmbeddingRequest, EmbeddingResponse> terminal =
            (actualRequest, actualToken) =>
            {
                Assert.Same(request, actualRequest);
                Assert.Equal(cancellation.Token, actualToken);
                return Task.FromResult(response);
            };
        var pipeline = new AsyncPipeline<EmbeddingRequest, EmbeddingResponse>(
            terminal,
            [EmbeddingValidation.RequireTextCardinality()]);

        var actual = await pipeline.InvokeAsync(request, cancellation.Token);

        Assert.Same(response, actual);
    }

    [Fact]
    public async Task TextValidationRejectsMissingVectors()
    {
        var request = new EmbeddingRequest(["first", "second"]);
        var response = new EmbeddingResponse(
            [new EmbeddingVector(new float[] { 1 })]);
        var pipeline = new AsyncPipeline<EmbeddingRequest, EmbeddingResponse>(
            (_, _) => Task.FromResult(response),
            [EmbeddingValidation.RequireTextCardinality()]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            pipeline.InvokeAsync(request));
    }

    [Fact]
    public async Task MultimodalValidationRejectsExtraVectors()
    {
        var request = new MultimodalEmbeddingRequest(
            [new MultimodalEmbeddingInput([new TextContent("first")])]);
        var response = new MultimodalEmbeddingResponse(
            [new EmbeddingVector(new float[] { 1 }),
                new EmbeddingVector(new float[] { 2 })]);
        var pipeline = new AsyncPipeline<MultimodalEmbeddingRequest,
            MultimodalEmbeddingResponse>(
            (_, _) => Task.FromResult(response),
            [EmbeddingValidation.RequireMultimodalCardinality()]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            pipeline.InvokeAsync(request));
    }

    [Fact]
    public async Task ValidationUsesRequestAtItsMiddlewarePosition()
    {
        var original = new EmbeddingRequest(["first", "second"]);
        var selected = new EmbeddingRequest(["first"]);
        var response = new EmbeddingResponse(
            [new EmbeddingVector(new float[] { 1 })]);
        AsyncMiddleware<EmbeddingRequest, EmbeddingResponse> select = next =>
            (_, token) => next(selected, token);
        var pipeline = new AsyncPipeline<EmbeddingRequest, EmbeddingResponse>(
            (request, _) =>
            {
                Assert.Same(selected, request);
                return Task.FromResult(response);
            },
            [select, EmbeddingValidation.RequireTextCardinality()]);

        var actual = await pipeline.InvokeAsync(original);

        Assert.Same(response, actual);
    }

    [Fact]
    public async Task MultimodalValidationRejectsMissingResponse()
    {
        var request = new MultimodalEmbeddingRequest(
            [new MultimodalEmbeddingInput([new TextContent("first")])]);
        var pipeline = new AsyncPipeline<MultimodalEmbeddingRequest,
            MultimodalEmbeddingResponse>(
            (_, _) => Task.FromResult<MultimodalEmbeddingResponse>(null!),
            [EmbeddingValidation.RequireMultimodalCardinality()]);

        var failure = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            pipeline.InvokeAsync(request));

        Assert.Equal(
            "The multimodal embedding client returned no response.",
            failure.Message);
    }
}
