namespace Crystal.Media;

/// <summary>Represents one provider-neutral media source.</summary>
public abstract class MediaSource
{
    private protected MediaSource()
    {
    }

    /// <summary>Gets the source access shape.</summary>
    public abstract MediaSourceKind Kind { get; }

    /// <summary>Gets the exact byte length when the caller knows it.</summary>
    public abstract long? Length { get; }

    /// <summary>Gets the source expiration time when known.</summary>
    public abstract DateTimeOffset? ExpiresAt { get; }

    /// <inheritdoc />
    public sealed override string ToString() => GetType().Name;
}
