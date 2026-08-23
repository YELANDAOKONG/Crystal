namespace Crystal.Completions;

/// <summary>
/// Defines a provider-neutral text-completion capability.
/// </summary>
public interface ICompletionClient
{
    /// <summary>
    /// Completes one caller-authored text prompt.
    /// </summary>
    /// <param name="request">The completion request.</param>
    /// <param name="cancellationToken">
    /// A token that cancels the provider operation.
    /// </param>
    /// <returns>The completion response.</returns>
    Task<CompletionResponse> CompleteAsync(
        CompletionRequest request,
        CancellationToken cancellationToken = default);
}
