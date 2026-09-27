using Crystal.Multimodal.Tools;

namespace Crystal.Realtime;

/// <summary>Sends one exact caller-owned result for a received tool call.</summary>
public sealed record RealtimeInputToolResult : RealtimeInputEvent
{
    /// <summary>Initializes a correlated tool result input.</summary>
    public RealtimeInputToolResult(MultimodalToolResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        Result = result;
    }

    /// <summary>Gets the exact caller-owned tool result.</summary>
    public MultimodalToolResult Result { get; }
}
