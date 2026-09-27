using Crystal.Multimodal;

namespace Crystal.Generation.Streaming;

/// <summary>Reports one complete, provisional typed media preview.</summary>
/// <typeparam name="TResponse">The target-specific complete response type.</typeparam>
public sealed record GenerationMediaPreviewReceived<TResponse> :
    GenerationStreamEvent<TResponse>
    where TResponse : class
{
    /// <summary>Initializes one complete provisional preview.</summary>
    /// <param name="candidateIndex">The zero-based candidate index.</param>
    /// <param name="itemIndex">The zero-based item index.</param>
    /// <param name="revisionIndex">The zero-based preview revision index.</param>
    /// <param name="content">A complete image, audio, or video content value.</param>
    public GenerationMediaPreviewReceived(
        int candidateIndex,
        int itemIndex,
        int revisionIndex,
        MultimodalContent content)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(candidateIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(itemIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(revisionIndex);
        ArgumentNullException.ThrowIfNull(content);

        if (content is not ImageContent and not AudioContent and not VideoContent)
        {
            throw new ArgumentException("A preview must contain media.", nameof(content));
        }

        CandidateIndex = candidateIndex;
        ItemIndex = itemIndex;
        RevisionIndex = revisionIndex;
        Content = content;
    }

    /// <summary>Gets the zero-based candidate index.</summary>
    public int CandidateIndex { get; }

    /// <summary>Gets the zero-based item index.</summary>
    public int ItemIndex { get; }

    /// <summary>Gets the zero-based preview revision index.</summary>
    public int RevisionIndex { get; }

    /// <summary>Gets the complete provisional media value.</summary>
    public MultimodalContent Content { get; }
}
