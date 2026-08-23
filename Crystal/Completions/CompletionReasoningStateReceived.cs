using Crystal.Reasoning;

namespace Crystal.Completions;

/// <summary>
/// Carries one complete opaque reasoning state for a completion item.
/// </summary>
public sealed record CompletionReasoningStateReceived : CompletionItemStreamEvent
{
    /// <summary>
    /// Initializes an opaque-state event.
    /// </summary>
    /// <param name="candidateIndex">The zero-based candidate index.</param>
    /// <param name="itemIndex">The zero-based item index.</param>
    /// <param name="state">The complete opaque state.</param>
    public CompletionReasoningStateReceived(
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
