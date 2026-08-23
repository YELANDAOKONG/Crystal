using Crystal.Internal;

namespace Crystal.Chat;

/// <summary>
/// Contains one ordered text-chat candidate.
/// </summary>
public sealed record ChatCandidate
{
    /// <summary>
    /// Initializes a chat candidate.
    /// </summary>
    /// <param name="items">The ordered candidate items.</param>
    /// <param name="finishReason">The provider-reported finish reason.</param>
    public ChatCandidate(
        IEnumerable<ChatItem> items,
        FinishReason finishReason)
    {
        ArgumentNullException.ThrowIfNull(finishReason, nameof(finishReason));

        Items = CollectionSnapshot.Create(items, nameof(items));
        FinishReason = finishReason;
    }

    /// <summary>
    /// Gets the ordered candidate items.
    /// </summary>
    public IReadOnlyList<ChatItem> Items { get; }

    /// <summary>
    /// Gets the finish reason.
    /// </summary>
    public FinishReason FinishReason { get; }
}
