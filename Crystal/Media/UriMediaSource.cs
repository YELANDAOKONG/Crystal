namespace Crystal.Media;

/// <summary>References media through an absolute caller-supplied URI.</summary>
public sealed class UriMediaSource : MediaSource, IEquatable<UriMediaSource>
{
    /// <summary>Initializes a URI media source.</summary>
    /// <param name="uri">The absolute URI, which Crystal does not fetch.</param>
    /// <param name="length">The optional exact byte length.</param>
    /// <param name="expiresAt">The optional source expiration time.</param>
    public UriMediaSource(
        Uri uri,
        long? length = null,
        DateTimeOffset? expiresAt = null)
    {
        ArgumentNullException.ThrowIfNull(uri, nameof(uri));

        if (!uri.IsAbsoluteUri)
        {
            throw new ArgumentException(
                "Media URI must be absolute.",
                nameof(uri));
        }

        if (length is < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(length),
                length,
                "Media length cannot be negative.");
        }

        Uri = uri;
        Length = length;
        ExpiresAt = expiresAt;
    }

    /// <inheritdoc />
    public override MediaSourceKind Kind => MediaSourceKind.Uri;

    /// <inheritdoc />
    public override long? Length { get; }

    /// <inheritdoc />
    public override DateTimeOffset? ExpiresAt { get; }

    /// <summary>Gets the exact absolute URI.</summary>
    public Uri Uri { get; }

    /// <inheritdoc />
    public bool Equals(UriMediaSource? other) =>
        other is not null && Uri.Equals(other.Uri) && Length == other.Length
        && ExpiresAt == other.ExpiresAt;

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is UriMediaSource other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Uri, Length, ExpiresAt);
}
