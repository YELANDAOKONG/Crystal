using Crystal.Multimodal.Tools;

namespace Crystal.Realtime;

/// <summary>Reports one complete model-authored tool call.</summary>
public sealed record RealtimeOutputToolCallReceived : RealtimeOutputEvent
{
    /// <summary>Initializes an exact tool-call output event.</summary>
    public RealtimeOutputToolCallReceived(MultimodalToolCall call)
    {
        ArgumentNullException.ThrowIfNull(call);
        Call = call;
    }

    /// <summary>Gets the exact model-authored call.</summary>
    public MultimodalToolCall Call { get; }
}
