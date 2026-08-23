using Crystal.Internal;

namespace Crystal.Completions;

/// <summary>
/// Contains one text-completion response.
/// </summary>
public sealed record CompletionResponse
{
    /// <summary>
    /// Initializes a completion response.
    /// </summary>
    /// <param name="candidates">The non-empty ordered candidates.</param>
    /// <param name="usage">Optional provider-reported token usage.</param>
    public CompletionResponse(
        IEnumerable<CompletionCandidate> candidates,
        TokenUsage? usage = null)
    {
        Candidates = CollectionSnapshot.Create(
            candidates,
            nameof(candidates),
            allowEmpty: false);
        Usage = usage;
    }

    /// <summary>
    /// Gets the ordered candidates.
    /// </summary>
    public IReadOnlyList<CompletionCandidate> Candidates { get; }

    /// <summary>
    /// Gets provider-reported usage when available.
    /// </summary>
    public TokenUsage? Usage { get; }
}
