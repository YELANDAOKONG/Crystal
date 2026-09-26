using Crystal.Embeddings;
using Crystal.Internal;

namespace Crystal.Multimodal.Embeddings;

/// <summary>Contains one vector per input in the original batch order.</summary>
public sealed record MultimodalEmbeddingResponse
{
    /// <summary>Initializes a multimodal embedding response.</summary>
    /// <param name="vectors">The non-empty vectors in input order.</param>
    /// <param name="usage">Optional provider-reported token usage.</param>
    public MultimodalEmbeddingResponse(
        IEnumerable<EmbeddingVector> vectors,
        TokenUsage? usage = null)
    {
        Vectors = CollectionSnapshot.Create(
            vectors,
            nameof(vectors),
            allowEmpty: false);
        Usage = usage;
    }

    /// <summary>Gets the vectors in input order.</summary>
    public IReadOnlyList<EmbeddingVector> Vectors { get; }

    /// <summary>Gets token usage only when the provider reports it.</summary>
    public TokenUsage? Usage { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(MultimodalEmbeddingResponse);
}
