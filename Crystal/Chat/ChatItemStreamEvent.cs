namespace Crystal.Chat;

/// <summary>
/// Represents a chat-stream event for one ordered candidate item.
/// </summary>
public abstract record ChatItemStreamEvent : ChatCandidateStreamEvent
{
    private protected ChatItemStreamEvent(
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
