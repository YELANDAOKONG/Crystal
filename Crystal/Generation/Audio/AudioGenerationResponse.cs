using Crystal.Internal;

namespace Crystal.Generation.Audio;

/// <summary>Contains one immediate audio-generation response.</summary>
public sealed record AudioGenerationResponse
{
    /// <summary>Initializes an immediate audio-generation response.</summary>
    /// <param name="candidates">The ordered candidates.</param>
    /// <param name="usage">Optional provider-reported token usage.</param>
    public AudioGenerationResponse(
        IEnumerable<GenerationCandidate> candidates,
        TokenUsage? usage = null)
    {
        Candidates = CollectionSnapshot.Create(candidates, nameof(candidates));
        Usage = usage;
    }

    /// <summary>Gets the ordered candidates, which may be empty.</summary>
    public IReadOnlyList<GenerationCandidate> Candidates { get; }

    /// <summary>Gets provider-reported token usage when available.</summary>
    public TokenUsage? Usage { get; }
}
