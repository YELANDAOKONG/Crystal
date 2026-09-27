namespace Crystal.Generation.Operations;

/// <summary>
/// Carries adapter-owned data needed to resume polling a generation operation.
/// Crystal does not interpret, store, or log the encoded data.
/// </summary>
public sealed class GenerationOperationTicket : IEquatable<GenerationOperationTicket>
{
    private readonly byte[] _data;

    /// <summary>Initializes an opaque operation ticket.</summary>
    /// <param name="format">A stable adapter-defined encoding identifier.</param>
    /// <param name="data">The complete encoded ticket.</param>
    public GenerationOperationTicket(string format, ReadOnlyMemory<byte> data)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(format);

        if (data.IsEmpty)
        {
            throw new ArgumentException("An operation ticket cannot be empty.", nameof(data));
        }

        Format = format;
        _data = data.ToArray();
    }

    /// <summary>Gets the adapter-defined encoding identifier.</summary>
    public string Format { get; }

    /// <summary>Gets a copy of the encoded ticket.</summary>
    public ReadOnlyMemory<byte> Data => _data.ToArray();

    /// <inheritdoc />
    public bool Equals(GenerationOperationTicket? other) =>
        other is not null
        && string.Equals(Format, other.Format, StringComparison.Ordinal)
        && _data.AsSpan().SequenceEqual(other._data);

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is GenerationOperationTicket other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Format, StringComparer.Ordinal);

        foreach (var value in _data)
        {
            hash.Add(value);
        }

        return hash.ToHashCode();
    }

    /// <inheritdoc />
    public override string ToString() => nameof(GenerationOperationTicket);
}
