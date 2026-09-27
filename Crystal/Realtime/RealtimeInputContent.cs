using Crystal.Multimodal;

namespace Crystal.Realtime;

/// <summary>Sends one exact text or independently decodable media segment.</summary>
public sealed record RealtimeInputContent : RealtimeInputEvent
{
    /// <summary>Initializes an exact live input segment.</summary>
    /// <param name="content">One complete typed content segment.</param>
    /// <param name="mediaOffset">Optional non-negative source media offset.</param>
    public RealtimeInputContent(
        MultimodalContent content,
        TimeSpan? mediaOffset = null)
    {
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

        Content = content;
        MediaOffset = mediaOffset;
    }

    /// <summary>Gets the exact caller-owned content segment.</summary>
    public MultimodalContent Content { get; }

    /// <summary>Gets the source media offset when supplied.</summary>
    public TimeSpan? MediaOffset { get; }
}
