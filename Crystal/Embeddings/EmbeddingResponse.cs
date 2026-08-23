using Crystal.Internal;

namespace Crystal.Embeddings;

/// <summary>
/// Contains vectors returned for an ordered embedding request.
/// </summary>
public sealed record EmbeddingResponse
{
    /// <summary>
    /// Initializes an embedding response.
    /// </summary>
    /// <param name="vectors">
    /// The non-empty vectors in their corresponding input order.
    /// </param>
    /// <param name="usage">Optional provider-reported token usage.</param>
    public EmbeddingResponse(
        IEnumerable<EmbeddingVector> vectors,
        TokenUsage? usage = null)
    {
        Vectors = CollectionSnapshot.Create(
            vectors,
            nameof(vectors),
            allowEmpty: false);
        Usage = usage;
    }

    /// <summary>
    /// Gets the vectors in input order.
    /// </summary>
    public IReadOnlyList<EmbeddingVector> Vectors { get; }

    /// <summary>
    /// Gets provider-reported usage when available.
    /// </summary>
    public TokenUsage? Usage { get; }
}
