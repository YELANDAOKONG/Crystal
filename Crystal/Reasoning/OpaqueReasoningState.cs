namespace Crystal.Reasoning;

/// <summary>
/// Preserves adapter-owned reasoning continuation data without interpreting it.
/// </summary>
public sealed class OpaqueReasoningState : IEquatable<OpaqueReasoningState>
{
    private readonly byte[] _data;

    /// <summary>
    /// Initializes opaque continuation state.
    /// </summary>
    /// <param name="format">
    /// A stable adapter-defined identifier for the encoded representation.
    /// </param>
    /// <param name="data">The complete encoded representation.</param>
    public OpaqueReasoningState(string format, ReadOnlyMemory<byte> data)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(format, nameof(format));

        if (data.IsEmpty)
        {
            throw new ArgumentException(
                "Opaque reasoning state cannot be empty.",
                nameof(data));
        }

        Format = format;
        _data = data.ToArray();
    }

    /// <summary>
    /// Gets the adapter-defined format identifier.
    /// </summary>
    public string Format { get; }

    /// <summary>
    /// Gets a copy of the opaque bytes.
    /// </summary>
    public ReadOnlyMemory<byte> Data => _data.ToArray();

    /// <inheritdoc />
    public bool Equals(OpaqueReasoningState? other) =>
        other is not null
        && string.Equals(Format, other.Format, StringComparison.Ordinal)
        && _data.AsSpan().SequenceEqual(other._data);

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is OpaqueReasoningState other && Equals(other);

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
    public override string ToString() => nameof(OpaqueReasoningState);
}
