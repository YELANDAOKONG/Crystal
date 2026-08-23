using Crystal.Internal;

namespace Crystal.Reasoning;

/// <summary>
/// Preserves one provider-native reasoning block.
/// </summary>
public sealed record ReasoningContent
{
    /// <summary>
    /// Initializes one reasoning block.
    /// </summary>
    /// <param name="textSegments">
    /// The ordered readable text segments in the block.
    /// </param>
    /// <param name="state">
    /// The optional opaque continuation representation for the block.
    /// </param>
    public ReasoningContent(
        IEnumerable<ReasoningText>? textSegments = null,
        OpaqueReasoningState? state = null)
    {
        TextSegments = CollectionSnapshot.Create(
            textSegments ?? Array.Empty<ReasoningText>(),
            nameof(textSegments));

        if (TextSegments.Count == 0 && state is null)
        {
            throw new ArgumentException(
                "Reasoning content must contain readable text or opaque state.",
                nameof(textSegments));
        }

        State = state;
    }

    /// <summary>
    /// Gets the ordered readable text segments.
    /// </summary>
    public IReadOnlyList<ReasoningText> TextSegments { get; }

    /// <summary>
    /// Gets the optional opaque continuation state.
    /// </summary>
    public OpaqueReasoningState? State { get; }
}
