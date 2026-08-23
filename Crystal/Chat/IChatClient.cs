namespace Crystal.Chat;

/// <summary>
/// Defines a provider-neutral text-chat capability.
/// </summary>
public interface IChatClient
{
    /// <summary>
    /// Completes one ordered text-chat request.
    /// </summary>
    /// <param name="request">The chat request.</param>
    /// <param name="cancellationToken">
    /// A token that cancels the provider operation.
    /// </param>
    /// <returns>The chat response.</returns>
    Task<ChatResponse> CompleteAsync(
        ChatRequest request,
        CancellationToken cancellationToken = default);
}
