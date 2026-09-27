using System.Runtime.InteropServices;

using Crystal.Media;

namespace Crystal.Tests;

public sealed class MediaSourceTests
{
    [Fact]
    public void InlineSourceCopiesInputAndDoesNotExposeItsOwnedBytes()
    {
        byte[] input = [1, 2, 3];
        var source = new InlineMediaSource(input);
        input[0] = 9;

        var firstRead = source.Data;
        if (MemoryMarshal.TryGetArray(firstRead, out var accessibleCopy))
        {
            accessibleCopy.Array![accessibleCopy.Offset] = 8;
        }

        Assert.Equal([1, 2, 3], source.Data.ToArray());
        Assert.Equal(3, source.Length);
        Assert.Null(source.ExpiresAt);
    }

    [Fact]
    public async Task ReplayableSourceOpensFreshCallerOwnedStreamsWithExactToken()
    {
        var receivedTokens = new List<CancellationToken>();
        var openedStreams = new List<MemoryStream>();
        using var cancellation = new CancellationTokenSource();
        var expiration = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var source = new ReplayableStreamMediaSource(
            token =>
            {
                receivedTokens.Add(token);
                var stream = new MemoryStream([4, 5]);
                openedStreams.Add(stream);
                return ValueTask.FromResult<Stream>(stream);
            },
            length: 2,
            expiresAt: expiration);

        var first = await source.OpenReadAsync(cancellation.Token);
        var second = await source.OpenReadAsync(cancellation.Token);

        Assert.NotSame(first, second);
        Assert.All(receivedTokens, token => Assert.Equal(cancellation.Token, token));
        Assert.Equal(2, receivedTokens.Count);
        Assert.Equal(4, first.ReadByte());
        Assert.Equal(4, second.ReadByte());
        Assert.All(openedStreams, stream => Assert.True(stream.CanRead));
        Assert.Equal(2, source.Length);
        Assert.Equal(expiration, source.ExpiresAt);

        await first.DisposeAsync();
        await second.DisposeAsync();
    }

    [Fact]
    public async Task ReplayableSourceRejectsSeekableStreamPastBeginning()
    {
        var stream = new MemoryStream([1, 2, 3]);
        stream.Position = 2;
        var source = new ReplayableStreamMediaSource(
            _ => ValueTask.FromResult<Stream>(stream));

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await source.OpenReadAsync());

        Assert.False(stream.CanRead);
    }
}
