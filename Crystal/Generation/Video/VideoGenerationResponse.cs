using Crystal.Internal;

namespace Crystal.Generation.Video;

/// <summary>
/// Contains one immediate video-generation response. Separate audio output is
/// represented by a distinct ordered audio content item; embedded audio is
/// reported by the generated video value.
/// </summary>
public sealed record VideoGenerationResponse
{
    /// <summary>Initializes an immediate video-generation response.</summary>
    /// <param name="candidates">The ordered candidates.</param>
    /// <param name="usage">Optional provider-reported token usage.</param>
    public VideoGenerationResponse(
        IEnumerable<GenerationCandidate> candidates,
        TokenUsage? usage = null)
    {
        Candidates = CollectionSnapshot.Create(candidates, nameof(candidates));
        Usage = usage;
    }

    /// <summary>Gets the non-empty ordered candidates.</summary>
    public IReadOnlyList<GenerationCandidate> Candidates { get; }

    /// <summary>Gets provider-reported token usage when available.</summary>
    public TokenUsage? Usage { get; }
}
