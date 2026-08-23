using Crystal.Internal;

namespace Crystal.Chat;

/// <summary>
/// Contains one text-chat response.
/// </summary>
public sealed record ChatResponse
{
    /// <summary>
    /// Initializes a chat response.
    /// </summary>
    /// <param name="candidates">The non-empty ordered candidates.</param>
    /// <param name="usage">Optional provider-reported token usage.</param>
    public ChatResponse(
        IEnumerable<ChatCandidate> candidates,
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
    public IReadOnlyList<ChatCandidate> Candidates { get; }

    /// <summary>
    /// Gets provider-reported usage when available.
    /// </summary>
    public TokenUsage? Usage { get; }
}
