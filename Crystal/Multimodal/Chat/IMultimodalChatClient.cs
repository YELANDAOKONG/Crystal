namespace Crystal.Multimodal.Chat;

/// <summary>Defines a provider-neutral, non-streaming multimodal Chat client.</summary>
public interface IMultimodalChatClient
{
    /// <summary>Gets the client's portable input and output support.</summary>
    MultimodalChatCapabilities Capabilities { get; }

    /// <summary>Completes one exact ordered multimodal Chat request.</summary>
    /// <param name="request">The exact request.</param>
    /// <param name="cancellationToken">A token that cancels provider work.</param>
    /// <returns>The complete response.</returns>
    Task<MultimodalChatResponse> CompleteAsync(
        MultimodalChatRequest request,
        CancellationToken cancellationToken = default);
}
