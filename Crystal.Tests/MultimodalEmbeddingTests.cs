using Crystal.Decorators;
using Crystal.Embeddings;
using Crystal.Media;
using Crystal.Multimodal;
using Crystal.Multimodal.Embeddings;
using Crystal.Pipelines;

namespace Crystal.Tests;

public sealed class MultimodalEmbeddingTests : IMultimodalEmbeddingClient
{
    private static readonly MultimodalContentCapability _text =
        new(ContentModality.Text);
    private static readonly MultimodalContentCapability _image =
        new(ContentModality.Image, [MediaSourceKind.Inline]);

    private readonly MultimodalEmbeddingResponse _response = new(
        [new EmbeddingVector(new float[] { 1, 2 })]);

    public MultimodalEmbeddingCapabilities Capabilities { get; } =
        new([_text, _image]);

    public MultimodalEmbeddingRequest? LastRequest { get; private set; }

    public CancellationToken LastToken { get; private set; }

    [Fact]
    public void InputsAndVectorsPreserveOrderWithoutExposingMutableCollections()
    {
        var text = new TextContent("exact text");
        var image = new ImageContent(new ImageMedia(
            new InlineMediaSource(new byte[] { 1, 2, 3 }),
            new MediaMimeType("image/png")));
        var blocks = new List<MultimodalContent> { text, image };
        var first = new MultimodalEmbeddingInput(blocks);
        blocks.Clear();
        var second = new MultimodalEmbeddingInput([new TextContent("next")]);
        var inputs = new List<MultimodalEmbeddingInput> { first, second };
        var request = new MultimodalEmbeddingRequest(inputs);
        inputs.Clear();

        Assert.Equal(2, request.Inputs.Count);
        Assert.Same(text, request.Inputs[0].Content[0]);
        Assert.Same(image, request.Inputs[0].Content[1]);
        Assert.Same(second, request.Inputs[1]);
        Assert.Equal(nameof(MultimodalEmbeddingRequest), request.ToString());

        var firstVector = new EmbeddingVector(new float[] { 1, 2 });
        var secondVector = new EmbeddingVector(new float[] { 3, 4 });
        var vectors = new List<EmbeddingVector> { firstVector, secondVector };
        var response = new MultimodalEmbeddingResponse(vectors);
        vectors.Clear();

        Assert.Same(firstVector, response.Vectors[0]);
        Assert.Same(secondVector, response.Vectors[1]);
        Assert.Equal(nameof(MultimodalEmbeddingResponse), response.ToString());
    }

    [Fact]
    public void CapabilitiesRejectDuplicateModalities()
    {
        var failure = Assert.Throws<ArgumentException>(() =>
            new MultimodalEmbeddingCapabilities([_text, _text]));

        Assert.Equal("inputs", failure.ParamName);
    }

    [Fact]
    public async Task DecoratorPreservesRequestResponseCapabilitiesAndCancellation()
    {
        MultimodalEmbeddingRequest? observed = null;
        AsyncMiddleware<MultimodalEmbeddingRequest, MultimodalEmbeddingResponse>
            middleware = next => async (request, token) =>
            {
                observed = request;
                return await next(request, token).ConfigureAwait(false);
            };
        var wrapped = Clients.ForMultimodalEmbedding(this, [middleware]);
        var request = new MultimodalEmbeddingRequest(
            [new MultimodalEmbeddingInput([new TextContent("input")])]);
        using var cancellation = new CancellationTokenSource();

        var response = await wrapped.EmbedAsync(request, cancellation.Token);

        Assert.Same(Capabilities, wrapped.Capabilities);
        Assert.Same(request, observed);
        Assert.Same(request, LastRequest);
        Assert.Same(_response, response);
        Assert.Equal(cancellation.Token, LastToken);
    }

    public Task<MultimodalEmbeddingResponse> EmbedAsync(
        MultimodalEmbeddingRequest request,
        CancellationToken cancellationToken = default)
    {
        LastRequest = request;
        LastToken = cancellationToken;
        return Task.FromResult(_response);
    }
}
