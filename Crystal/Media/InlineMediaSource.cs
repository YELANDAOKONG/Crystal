namespace Crystal.Media;

/// <summary>Owns an immutable copy of complete inline media bytes.</summary>
public sealed class InlineMediaSource : MediaSource, IEquatable<InlineMediaSource>
{
    private readonly byte[] _data;

    /// <summary>Initializes an inline media source by copying its bytes.</summary>
    /// <param name="data">The non-empty complete media bytes.</param>
    public InlineMediaSource(ReadOnlyMemory<byte> data)
    {
        if (data.IsEmpty)
        {
            throw new ArgumentException(
                "Inline media data cannot be empty.",
                nameof(data));
        }

        _data = data.ToArray();
    }

    /// <inheritdoc />
    public override MediaSourceKind Kind => MediaSourceKind.Inline;

    /// <inheritdoc />
    public override long? Length => _data.LongLength;

    /// <inheritdoc />
    public override DateTimeOffset? ExpiresAt => null;

    /// <summary>Gets a copy of the complete media bytes.</summary>
    public ReadOnlyMemory<byte> Data => _data.ToArray();

    /// <inheritdoc />
    public bool Equals(InlineMediaSource? other) =>
        other is not null && _data.AsSpan().SequenceEqual(other._data);

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is InlineMediaSource other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();

        foreach (var value in _data)
        {
            hash.Add(value);
        }

        return hash.ToHashCode();
    }
}
