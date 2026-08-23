namespace Crystal.Embeddings;

/// <summary>
/// Contains one immutable embedding vector.
/// </summary>
public sealed class EmbeddingVector : IEquatable<EmbeddingVector>
{
    private readonly float[] _values;

    /// <summary>
    /// Initializes an embedding vector by copying its values.
    /// </summary>
    /// <param name="values">The non-empty vector values.</param>
    public EmbeddingVector(ReadOnlyMemory<float> values)
    {
        if (values.IsEmpty)
        {
            throw new ArgumentException(
                "Embedding vector cannot be empty.",
                nameof(values));
        }

        _values = values.ToArray();
    }

    /// <summary>
    /// Gets the number of vector dimensions.
    /// </summary>
    public int Dimensions => _values.Length;

    /// <summary>
    /// Gets a copy of the vector values.
    /// </summary>
    public ReadOnlyMemory<float> Values => _values.ToArray();

    /// <inheritdoc />
    public bool Equals(EmbeddingVector? other) =>
        other is not null && _values.AsSpan().SequenceEqual(other._values);

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is EmbeddingVector other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();

        foreach (var value in _values)
        {
            hash.Add(value);
        }

        return hash.ToHashCode();
    }
}
