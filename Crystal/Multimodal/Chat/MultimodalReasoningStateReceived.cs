using Crystal.Reasoning;

namespace Crystal.Multimodal.Chat;

/// <summary>Carries one complete opaque state for a reasoning item.</summary>
public sealed record MultimodalReasoningStateReceived
    : MultimodalChatItemStreamEvent
{
    /// <summary>Initializes an opaque reasoning-state event.</summary>
    /// <param name="candidateIndex">The zero-based candidate index.</param>
    /// <param name="itemIndex">The zero-based item index.</param>
    /// <param name="state">The complete opaque state.</param>
    public MultimodalReasoningStateReceived(
        int candidateIndex,
        int itemIndex,
        OpaqueReasoningState state)
        : base(candidateIndex, itemIndex)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        State = state;
    }

    /// <summary>Gets the complete opaque state.</summary>
    public OpaqueReasoningState State { get; }
}
