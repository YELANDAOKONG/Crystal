namespace Crystal.Chat;

/// <summary>
/// Adds typed streaming to a text-chat client.
/// </summary>
public interface IStreamingChatClient : IChatClient
{
    /// <summary>
    /// Streams one text-chat operation.
    /// </summary>
    /// <param name="request">The chat request.</param>
    /// <param name="cancellationToken">
    /// A token that cancels stream enumeration and provider work.
    /// </param>
    /// <returns>The ordered chat events.</returns>
    IAsyncEnumerable<ChatStreamEvent> StreamAsync(
        ChatRequest request,
        CancellationToken cancellationToken = default);
}
