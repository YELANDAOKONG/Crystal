namespace Crystal.Completions;

/// <summary>
/// Represents a completion-stream event for one ordered candidate item.
/// </summary>
public abstract record CompletionItemStreamEvent : CompletionCandidateStreamEvent
{
    private protected CompletionItemStreamEvent(
        int candidateIndex,
        int itemIndex)
        : base(candidateIndex)
    {
        if (itemIndex < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(itemIndex),
                itemIndex,
                "Item index cannot be negative.");
        }

        ItemIndex = itemIndex;
    }

    /// <summary>
    /// Gets the zero-based item index.
    /// </summary>
    public int ItemIndex { get; }
}
