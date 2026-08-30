using Crystal.Internal;

namespace Crystal.Multimodal.Chat;

/// <summary>Contains one complete multimodal Chat response.</summary>
public sealed record MultimodalChatResponse
{
    /// <summary>Initializes a complete multimodal Chat response.</summary>
    /// <param name="candidates">The non-empty ordered candidates.</param>
    /// <param name="usage">Optional provider-reported token usage.</param>
    public MultimodalChatResponse(
        IEnumerable<MultimodalChatCandidate> candidates,
        TokenUsage? usage = null)
    {
        Candidates = CollectionSnapshot.Create(
            candidates,
            nameof(candidates),
            allowEmpty: false);
        Usage = usage;
    }

    /// <summary>Gets the non-empty ordered candidates.</summary>
    public IReadOnlyList<MultimodalChatCandidate> Candidates { get; }

    /// <summary>Gets provider-reported token usage when available.</summary>
    public TokenUsage? Usage { get; }
}
