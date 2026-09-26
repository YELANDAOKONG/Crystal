namespace Crystal.Multimodal.Embeddings;

/// <summary>Defines an optional provider-neutral multimodal embedding client.</summary>
public interface IMultimodalEmbeddingClient
{
    /// <summary>Gets the individual input shapes this client accepts.</summary>
    MultimodalEmbeddingCapabilities Capabilities { get; }

    /// <summary>Embeds an ordered batch of typed content inputs.</summary>
    /// <param name="request">The exact ordered request.</param>
    /// <param name="cancellationToken">A token that cancels provider work.</param>
    /// <returns>One ordered vector per accepted input.</returns>
    Task<MultimodalEmbeddingResponse> EmbedAsync(
        MultimodalEmbeddingRequest request,
        CancellationToken cancellationToken = default);
}
