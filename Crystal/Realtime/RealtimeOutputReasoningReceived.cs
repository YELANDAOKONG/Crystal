using Crystal.Multimodal;

namespace Crystal.Realtime;

/// <summary>Reports one exact readable or opaque reasoning block.</summary>
public sealed record RealtimeOutputReasoningReceived : RealtimeOutputEvent
{
    /// <summary>Initializes a correlated reasoning output event.</summary>
    public RealtimeOutputReasoningReceived(
        string outputId,
        int itemIndex,
        MultimodalReasoningContent content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputId);
        ArgumentOutOfRangeException.ThrowIfNegative(itemIndex);
        ArgumentNullException.ThrowIfNull(content);

        OutputId = outputId;
        ItemIndex = itemIndex;
        Content = content;
    }

    /// <summary>Gets the stable output identifier.</summary>
    public string OutputId { get; }

    /// <summary>Gets the zero-based item index within this output.</summary>
    public int ItemIndex { get; }

    /// <summary>Gets the exact reasoning block.</summary>
    public MultimodalReasoningContent Content { get; }
}
