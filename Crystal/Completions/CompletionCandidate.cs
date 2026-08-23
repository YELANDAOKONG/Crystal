using Crystal.Internal;

namespace Crystal.Completions;

/// <summary>
/// Contains one ordered completion candidate.
/// </summary>
public sealed record CompletionCandidate
{
    /// <summary>
    /// Initializes a completion candidate.
    /// </summary>
    /// <param name="items">The ordered text and reasoning items.</param>
    /// <param name="finishReason">The provider-reported finish reason.</param>
    public CompletionCandidate(
        IEnumerable<CompletionItem> items,
        FinishReason finishReason)
    {
        ArgumentNullException.ThrowIfNull(finishReason, nameof(finishReason));

        Items = CollectionSnapshot.Create(items, nameof(items));
        FinishReason = finishReason;
    }

    /// <summary>
    /// Gets the ordered output items.
    /// </summary>
    public IReadOnlyList<CompletionItem> Items { get; }

    /// <summary>
    /// Gets the finish reason.
    /// </summary>
    public FinishReason FinishReason { get; }
}
