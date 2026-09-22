namespace Crystal.Multimodal.Chat;

/// <summary>Adds typed streaming to a multimodal Chat client.</summary>
public interface IStreamingMultimodalChatClient : IMultimodalChatClient
{
    /// <summary>Streams one exact ordered multimodal Chat request.</summary>
    /// <param name="request">The exact request.</param>
    /// <param name="cancellationToken">
    /// A token that cancels stream enumeration and provider work.
    /// </param>
    /// <returns>The ordered multimodal Chat events.</returns>
    IAsyncEnumerable<MultimodalChatStreamEvent> StreamAsync(
        MultimodalChatRequest request,
        CancellationToken cancellationToken = default);
}
