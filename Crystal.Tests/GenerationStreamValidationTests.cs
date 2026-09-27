using Crystal.Decorators;
using Crystal.Generation.Images;
using Crystal.Generation.Streaming;
using Crystal.Media;
using Crystal.Multimodal;
using Crystal.Pipelines;

namespace Crystal.Tests;

public sealed class GenerationStreamValidationTests
{
    private static readonly MediaMimeType Png = new("image/png");
    private static readonly MediaMimeType Jpeg = new("image/jpeg");

    [Fact]
    public async Task ValidInterleavedRevisionsForwardExactEventsAndFinalResponse()
    {
        var request = new ImageGenerationRequest([]);
        var firstChunk = Chunk(0, 0, 0, 0, false);
        var preview = new GenerationMediaPreviewReceived<ImageGenerationResponse>(
            1, 0, 0,
            new ImageContent(new ImageMedia(
                new InlineMediaSource(new byte[] { 7 }), Png)));
        var lastChunk = Chunk(0, 0, 0, 1, true);
        var revised = Chunk(0, 0, 1, 0, true);
        var response = new ImageGenerationResponse([]);
        var completed = new GenerationStreamCompleted<ImageGenerationResponse>(response);
        var expected = new GenerationStreamEvent<ImageGenerationResponse>[]
        {
            firstChunk, preview, lastChunk, revised, completed
        };
        using var cancellation = new CancellationTokenSource();
        var pipeline = new StreamingPipeline<ImageGenerationRequest,
            GenerationStreamEvent<ImageGenerationResponse>>(
            (actualRequest, token) =>
            {
                Assert.Same(request, actualRequest);
                Assert.Equal(cancellation.Token, token);
                return Events(expected);
            },
            [GenerationStreamValidation.RequireProtocol<ImageGenerationRequest,
                ImageGenerationResponse>()]);

        var actual = await CollectAsync(
            pipeline.StreamAsync(request, cancellation.Token));

        Assert.Equal(expected.Length, actual.Count);
        for (var index = 0; index < expected.Length; index++)
        {
            Assert.Same(expected[index], actual[index]);
        }

        Assert.Same(response,
            Assert.IsType<GenerationStreamCompleted<ImageGenerationResponse>>(
                actual[^1]).Response);
    }

    [Fact]
    public async Task BrokenRevisionSequencesFailWithoutForwardingCompletion()
    {
        var response = new ImageGenerationResponse([]);
        var completed = new GenerationStreamCompleted<ImageGenerationResponse>(response);
        var invalidStreams = new GenerationStreamEvent<ImageGenerationResponse>[][]
        {
            [Chunk(0, 0, 0, 1, true), completed],
            [Chunk(0, 0, 0, 0, false), Chunk(0, 0, 0, 2, true), completed],
            [Chunk(0, 0, 0, 0, false), Chunk(0, 0, 0, 1, true, Jpeg), completed],
            [Chunk(0, 0, 0, 0, true), Chunk(0, 0, 0, 1, true), completed],
            [Chunk(0, 0, 0, 0, false), Chunk(0, 0, 1, 0, true), completed],
            [Chunk(0, 0, 0, 0, false), completed],
            [Chunk(0, 0, 0, 0, true)]
        };

        foreach (var events in invalidStreams)
        {
            var pipeline = Pipeline(events);
            var forwarded = new List<GenerationStreamEvent<ImageGenerationResponse>>();

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await foreach (var streamEvent in pipeline.StreamAsync(
                    new ImageGenerationRequest([])))
                {
                    forwarded.Add(streamEvent);
                }
            });

            Assert.DoesNotContain(completed, forwarded);
        }
    }

    [Fact]
    public async Task CompletionMustBeTheLastAndOnlyTerminalEvent()
    {
        var terminal = new GenerationStreamCompleted<ImageGenerationResponse>(
            new ImageGenerationResponse([]));
        var pipeline = Pipeline([terminal, terminal]);
        var forwarded = new List<GenerationStreamEvent<ImageGenerationResponse>>();

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await foreach (var streamEvent in pipeline.StreamAsync(
                new ImageGenerationRequest([])))
            {
                forwarded.Add(streamEvent);
            }
        });

        Assert.Empty(forwarded);
    }

    [Fact]
    public async Task EarlyDisposalDoesNotRequireACompletionEvent()
    {
        var firstChunk = Chunk(0, 0, 0, 0, false);
        var pipeline = Pipeline([firstChunk]);

        await using (var enumerator = pipeline.StreamAsync(
            new ImageGenerationRequest([])).GetAsyncEnumerator())
        {
            Assert.True(await enumerator.MoveNextAsync());
            Assert.Same(firstChunk, enumerator.Current);
        }
    }

    [Fact]
    public async Task ObservedCancellationDoesNotForwardLateCompletion()
    {
        using var cancellation = new CancellationTokenSource();
        var terminal = new GenerationStreamCompleted<ImageGenerationResponse>(
            new ImageGenerationResponse([]));
        var pipeline = new StreamingPipeline<ImageGenerationRequest,
            GenerationStreamEvent<ImageGenerationResponse>>(
            (_, _) => CancelThenYield(cancellation, terminal),
            [GenerationStreamValidation.RequireProtocol<ImageGenerationRequest,
                ImageGenerationResponse>()]);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CollectAsync(pipeline.StreamAsync(
                new ImageGenerationRequest([]), cancellation.Token)));
    }

    private static GenerationMediaChunkReceived<ImageGenerationResponse> Chunk(
        int candidate,
        int item,
        int revision,
        int index,
        bool isLast,
        MediaMimeType? mimeType = null) =>
        new(candidate, item, revision, index,
            ContentModality.Image, mimeType ?? Png, new byte[] { 1 }, isLast);

    private static StreamingPipeline<ImageGenerationRequest,
        GenerationStreamEvent<ImageGenerationResponse>> Pipeline(
            IEnumerable<GenerationStreamEvent<ImageGenerationResponse>> events) =>
        new((_, _) => Events(events),
            [GenerationStreamValidation.RequireProtocol<ImageGenerationRequest,
                ImageGenerationResponse>()]);

    private static async IAsyncEnumerable<GenerationStreamEvent<ImageGenerationResponse>>
        Events(IEnumerable<GenerationStreamEvent<ImageGenerationResponse>> events)
    {
        await Task.Yield();
        foreach (var streamEvent in events)
        {
            yield return streamEvent;
        }
    }

    private static async IAsyncEnumerable<GenerationStreamEvent<ImageGenerationResponse>>
        CancelThenYield(
            CancellationTokenSource cancellation,
            GenerationStreamCompleted<ImageGenerationResponse> terminal)
    {
        await Task.Yield();
        cancellation.Cancel();
        yield return terminal;
    }

    private static async Task<List<GenerationStreamEvent<ImageGenerationResponse>>>
        CollectAsync(
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
