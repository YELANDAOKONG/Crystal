using Crystal.Internal;

namespace Crystal.Multimodal.Embeddings;

/// <summary>Contains one ordered set of typed content to embed together.</summary>
public sealed record MultimodalEmbeddingInput
{
    /// <summary>Initializes one embedding input.</summary>
    /// <param name="content">The non-empty ordered content blocks.</param>
    public MultimodalEmbeddingInput(IEnumerable<MultimodalContent> content)
    {
        Content = CollectionSnapshot.Create(
            content,
            nameof(content),
            allowEmpty: false);
    }

    /// <summary>Gets the exact ordered content blocks.</summary>
    public IReadOnlyList<MultimodalContent> Content { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(MultimodalEmbeddingInput);
}
