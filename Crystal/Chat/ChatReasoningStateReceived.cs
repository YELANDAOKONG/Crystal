using Crystal.Reasoning;

namespace Crystal.Chat;

/// <summary>
/// Carries one complete opaque reasoning state for a chat item.
/// </summary>
public sealed record ChatReasoningStateReceived : ChatItemStreamEvent
{
    /// <summary>
    /// Initializes an opaque-state event.
    /// </summary>
    /// <param name="candidateIndex">The zero-based candidate index.</param>
    /// <param name="itemIndex">The zero-based item index.</param>
    /// <param name="state">The complete opaque state.</param>
    public ChatReasoningStateReceived(
        int candidateIndex,
        int itemIndex,
        OpaqueReasoningState state)
        : base(candidateIndex, itemIndex)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        State = state;
    }

    /// <summary>
    /// Gets the complete opaque state.
    /// </summary>
    public OpaqueReasoningState State { get; }
}
