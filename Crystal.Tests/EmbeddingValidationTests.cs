using Crystal.Decorators;
using Crystal.Embeddings;
using Crystal.Multimodal;
using Crystal.Multimodal.Embeddings;
using Crystal.Pipelines;

namespace Crystal.Tests;

public sealed class EmbeddingValidationTests
{
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
