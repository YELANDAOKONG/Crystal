using Crystal.Reasoning;

namespace Crystal.Generation.Images;

/// <summary>Contains one immediate image-generation request.</summary>
public sealed record ImageGenerationRequest
{
    /// <summary>Initializes an immediate image-generation request.</summary>
    /// <param name="inputs">The ordered typed inputs.</param>
    /// <param name="requirements">Optional portable hard requirements.</param>
    /// <param name="requestedCandidateCount">Optional positive requested candidate count.</param>
    /// <param name="reasoning">Optional portable reasoning hints.</param>
    public ImageGenerationRequest(
        IEnumerable<GenerationInput> inputs,
        ImageGenerationRequirements? requirements = null,
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
    public ImageGenerationRequirements? Requirements { get; }

    /// <summary>Gets the requested candidate count.</summary>
    public int? RequestedCandidateCount { get; }

    /// <summary>Gets optional portable reasoning hints.</summary>
    public ReasoningOptions? Reasoning { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(ImageGenerationRequest);
}
