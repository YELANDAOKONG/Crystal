using System.Runtime.InteropServices;

using Crystal.Generation.Images;
using Crystal.Generation.Streaming;
using Crystal.Media;
using Crystal.Multimodal;

namespace Crystal.Tests;

public sealed class GenerationStreamingTests
{
    [Fact]
    public void EncodedChunkCopiesBytesAndRejectsNonMedia()
    {
        byte[] data = [1, 2, 3];
        var chunk = new GenerationMediaChunkReceived<ImageGenerationResponse>(
            0, 1, 2, 0,
            ContentModality.Image,
            new MediaMimeType("image/png"),
            data,
            isLastChunk: true);
        data[0] = 9;

        var firstRead = chunk.Data;
        if (MemoryMarshal.TryGetArray(firstRead, out var accessibleCopy))
        {
            accessibleCopy.Array![accessibleCopy.Offset] = 8;
        }

        Assert.Equal([1, 2, 3], chunk.Data.ToArray());
        Assert.Equal(2, chunk.RevisionIndex);
        Assert.True(chunk.IsLastChunk);
        Assert.Throws<ArgumentException>(() =>
            new GenerationMediaChunkReceived<ImageGenerationResponse>(
                0, 0, 0, 0,
                ContentModality.Text,
                new MediaMimeType("text/plain"),
                new byte[] { 1 },
                true));
    }

    [Fact]
    public void PreviewAndFinalResponseHaveDistinctSemantics()
    {
        var preview = new GenerationMediaPreviewReceived<ImageGenerationResponse>(
            0, 0, 0,
            new ImageContent(new ImageMedia(
                new InlineMediaSource(new byte[] { 1 }),
                new MediaMimeType("image/png"))));
        var response = new ImageGenerationResponse([]);
        var completed = new GenerationStreamCompleted<ImageGenerationResponse>(
            response);

        Assert.IsType<ImageContent>(preview.Content);
        Assert.Same(response, completed.Response);
        Assert.Throws<ArgumentException>(() =>
            new GenerationMediaPreviewReceived<ImageGenerationResponse>(
                0, 0, 0, new TextContent("not media")));
    }

    [Fact]
    public void EqualEncodedChunksHaveEqualHashesAndCopiedDataComparesByContent()
    {
        var first = new GenerationMediaChunkReceived<ImageGenerationResponse>(
            0, 1, 2, 3, ContentModality.Image,
            new MediaMimeType("image/png"), new byte[] { 1, 2 }, true);
        var equal = new GenerationMediaChunkReceived<ImageGenerationResponse>(
            0, 1, 2, 3, ContentModality.Image,
            new MediaMimeType("image/png"), new byte[] { 1, 2 }, true);
        var different = new GenerationMediaChunkReceived<ImageGenerationResponse>(
            0, 1, 2, 3, ContentModality.Image,
            new MediaMimeType("image/png"), new byte[] { 1, 3 }, true);

        Assert.Equal(first, equal);
        Assert.Equal(first.GetHashCode(), equal.GetHashCode());
        Assert.Single(new HashSet<GenerationMediaChunkReceived<ImageGenerationResponse>>
            { first, equal });
        Assert.NotEqual(first, different);
        Assert.True(first.Data.Span.SequenceEqual(first.Data.Span));
    }
}
