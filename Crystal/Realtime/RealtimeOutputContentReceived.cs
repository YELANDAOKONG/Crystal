using Crystal.Multimodal;

namespace Crystal.Realtime;

/// <summary>Reports one exact ordered text or media output segment.</summary>
public sealed record RealtimeOutputContentReceived : RealtimeOutputEvent
{
    /// <summary>Initializes a correlated output segment.</summary>
    /// <param name="outputId">A stable adapter-reported output identifier.</param>
    /// <param name="segmentIndex">Zero-based segment index in this output.</param>
    /// <param name="content">One complete typed output segment.</param>
    /// <param name="mediaOffset">Optional non-negative output media offset.</param>
    public RealtimeOutputContentReceived(
        string outputId,
        int segmentIndex,
        MultimodalContent content,
        TimeSpan? mediaOffset = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputId);
        ArgumentOutOfRangeException.ThrowIfNegative(segmentIndex);
        ArgumentNullException.ThrowIfNull(content);
        if (mediaOffset < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(mediaOffset));
        }

        if (content.Modality == ContentModality.Text && mediaOffset is not null)
        {
            throw new ArgumentException(
                "A text segment cannot have a media offset.",
                nameof(mediaOffset));
        }

        OutputId = outputId;
        SegmentIndex = segmentIndex;
        Content = content;
        MediaOffset = mediaOffset;
    }

    /// <summary>Gets the stable output identifier.</summary>
    public string OutputId { get; }

    /// <summary>Gets the zero-based segment index within this output.</summary>
    public int SegmentIndex { get; }

    /// <summary>Gets the exact typed output segment.</summary>
    public MultimodalContent Content { get; }

    /// <summary>Gets the output media offset when reported.</summary>
    public TimeSpan? MediaOffset { get; }
}
