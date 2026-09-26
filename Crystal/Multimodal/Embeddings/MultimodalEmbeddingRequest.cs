using Crystal.Internal;

namespace Crystal.Multimodal.Embeddings;

/// <summary>Contains an ordered batch of multimodal embedding inputs.</summary>
public sealed record MultimodalEmbeddingRequest
{
    /// <summary>Initializes a multimodal embedding request.</summary>
    /// <param name="inputs">The non-empty ordered input batch.</param>
    public MultimodalEmbeddingRequest(
        IEnumerable<MultimodalEmbeddingInput> inputs)
    {
        Inputs = CollectionSnapshot.Create(
            inputs,
            nameof(inputs),
            allowEmpty: false);
    }

    /// <summary>Gets the exact ordered embedding inputs.</summary>
    public IReadOnlyList<MultimodalEmbeddingInput> Inputs { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(MultimodalEmbeddingRequest);
}
