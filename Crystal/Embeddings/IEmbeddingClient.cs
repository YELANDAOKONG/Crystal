namespace Crystal.Embeddings;

/// <summary>
/// Defines a provider-neutral text-embedding capability.
/// </summary>
public interface IEmbeddingClient
{
    /// <summary>
    /// Embeds an ordered text batch.
    /// </summary>
    /// <param name="request">The embedding request.</param>
    /// <param name="cancellationToken">
    /// A token that cancels the provider operation.
    /// </param>
    /// <returns>The ordered embedding response.</returns>
    Task<EmbeddingResponse> EmbedAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken = default);
}
