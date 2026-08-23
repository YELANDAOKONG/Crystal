namespace Crystal.Completions;

/// <summary>
/// Adds typed streaming to a text-completion client.
/// </summary>
public interface IStreamingCompletionClient : ICompletionClient
{
    /// <summary>
    /// Streams one text-completion operation.
    /// </summary>
    /// <param name="request">The completion request.</param>
    /// <param name="cancellationToken">
    /// A token that cancels stream enumeration and provider work.
    /// </param>
    /// <returns>The ordered completion events.</returns>
    IAsyncEnumerable<CompletionStreamEvent> StreamAsync(
        CompletionRequest request,
        CancellationToken cancellationToken = default);
}
