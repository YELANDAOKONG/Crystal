using Crystal.Internal;
using Crystal.Reasoning;

namespace Crystal.Multimodal;

/// <summary>Preserves one readable or opaque multimodal reasoning block.</summary>
public sealed record MultimodalReasoningContent
{
    /// <summary>Initializes one multimodal reasoning block.</summary>
    /// <param name="parts">The ordered classified readable reasoning parts.</param>
    /// <param name="state">Optional opaque continuation state.</param>
    public MultimodalReasoningContent(
        IEnumerable<MultimodalReasoningPart>? parts = null,
        OpaqueReasoningState? state = null)
    {
        Parts = CollectionSnapshot.Create(
            parts ?? Array.Empty<MultimodalReasoningPart>(),
            nameof(parts));

        if (Parts.Count == 0 && state is null)
        {
            throw new ArgumentException(
                "Reasoning content must contain readable parts or opaque state.",
                nameof(parts));
        }

        State = state;
    }

    /// <summary>Gets the ordered classified readable reasoning parts.</summary>
    public IReadOnlyList<MultimodalReasoningPart> Parts { get; }

    /// <summary>Gets optional opaque continuation state.</summary>
    public OpaqueReasoningState? State { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(MultimodalReasoningContent);
}
