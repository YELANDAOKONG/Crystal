namespace Crystal.Media;

/// <summary>Opens a fresh readable media stream for every consumption attempt.</summary>
public sealed class ReplayableStreamMediaSource : MediaSource
{
    private readonly MediaStreamFactory _factory;

    /// <summary>Initializes a replayable stream media source.</summary>
    /// <param name="factory">
    /// A factory that returns a fresh readable stream at its beginning.
    /// Ownership of each returned stream transfers to the caller.
    /// </param>
    /// <param name="length">The optional exact byte length.</param>
    /// <param name="expiresAt">The optional source expiration time.</param>
    public ReplayableStreamMediaSource(
        MediaStreamFactory factory,
        long? length = null,
        DateTimeOffset? expiresAt = null)
    {
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));

        if (length is < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(length),
                length,
                "Media length cannot be negative.");
        }

        _factory = factory;
        Length = length;
        ExpiresAt = expiresAt;
    }

    /// <inheritdoc />
    public override MediaSourceKind Kind => MediaSourceKind.ReplayableStream;

    /// <inheritdoc />
    public override long? Length { get; }

    /// <inheritdoc />
    public override DateTimeOffset? ExpiresAt { get; }

    /// <summary>Opens a fresh readable stream at its beginning.</summary>
    /// <param name="cancellationToken">A token that cancels opening the stream.</param>
    /// <returns>A stream owned by the caller.</returns>
    public async ValueTask<Stream> OpenReadAsync(
        CancellationToken cancellationToken = default)
    {
        var stream = await _factory(cancellationToken).ConfigureAwait(false);

        if (stream is null)
        {
            throw new InvalidOperationException(
                "The media stream factory returned no stream.");
        }

        if (!stream.CanRead)
        {
            await stream.DisposeAsync().ConfigureAwait(false);
            throw new InvalidOperationException(
                "The media stream factory returned an unreadable stream.");
        }

        return stream;
    }
}
