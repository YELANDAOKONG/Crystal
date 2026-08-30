using Crystal.Reasoning;

namespace Crystal.Generation.Audio;

/// <summary>Contains one immediate audio-generation request.</summary>
public sealed record AudioGenerationRequest
{
    /// <summary>Initializes an immediate audio-generation request.</summary>
    /// <param name="inputs">The ordered typed inputs.</param>
    /// <param name="requirements">Optional portable hard requirements.</param>
    /// <param name="requestedCandidateCount">Optional positive requested candidate count.</param>
    /// <param name="reasoning">Optional portable reasoning hints.</param>
    public AudioGenerationRequest(
        IEnumerable<GenerationInput> inputs,
        AudioGenerationRequirements? requirements = null,
        int? requestedCandidateCount = null,
        ReasoningOptions? reasoning = null)
    {
        GenerationRequestRules.ValidateRequestedCandidateCount(requestedCandidateCount);

        Inputs = GenerationRequestRules.SnapshotInputs(inputs);
        Requirements = requirements;
        RequestedCandidateCount = requestedCandidateCount;
        Reasoning = reasoning;
    }

    /// <summary>Gets the ordered typed inputs.</summary>
    public IReadOnlyList<GenerationInput> Inputs { get; }

    /// <summary>Gets optional portable hard requirements.</summary>
    public AudioGenerationRequirements? Requirements { get; }

    /// <summary>Gets the requested candidate count.</summary>
    public int? RequestedCandidateCount { get; }

    /// <summary>Gets optional portable reasoning hints.</summary>
    public ReasoningOptions? Reasoning { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(AudioGenerationRequest);
}
