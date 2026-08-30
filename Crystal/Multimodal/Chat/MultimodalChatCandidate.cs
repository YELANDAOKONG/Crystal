using Crystal.Internal;

namespace Crystal.Multimodal.Chat;

/// <summary>Contains one ordered multimodal Chat candidate.</summary>
public sealed record MultimodalChatCandidate
{
    /// <summary>Initializes a multimodal Chat candidate.</summary>
    /// <param name="items">The exact ordered candidate items.</param>
    /// <param name="finishReason">The provider-reported finish reason.</param>
    public MultimodalChatCandidate(
        IEnumerable<MultimodalChatItem> items,
        FinishReason finishReason)
    {
        ArgumentNullException.ThrowIfNull(finishReason, nameof(finishReason));

        Items = CollectionSnapshot.Create(items, nameof(items));
        FinishReason = finishReason;
    }

    /// <summary>Gets the exact ordered candidate items.</summary>
    public IReadOnlyList<MultimodalChatItem> Items { get; }

    /// <summary>Gets the provider-reported finish reason.</summary>
    public FinishReason FinishReason { get; }
}
