using Crystal.Internal;

namespace Crystal.Multimodal.Embeddings;

/// <summary>Advertises individual supported embedding input shapes.</summary>
public sealed record MultimodalEmbeddingCapabilities
{
    /// <summary>Initializes multimodal embedding capabilities.</summary>
    /// <param name="inputs">The non-empty supported individual input shapes.</param>
    public MultimodalEmbeddingCapabilities(
        IEnumerable<MultimodalContentCapability> inputs)
    {
        Inputs = CollectionSnapshot.Create(
            inputs,
            nameof(inputs),
            allowEmpty: false);

        if (Inputs.Select(static input => input.Modality)
            .Distinct()
            .Count() != Inputs.Count)
        {
            throw new ArgumentException(
                "Multimodal embedding input modalities must be unique.",
                nameof(inputs));
        }
    }

    /// <summary>Gets supported individual modalities and source shapes.</summary>
    public IReadOnlyList<MultimodalContentCapability> Inputs { get; }
}
