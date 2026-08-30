using Crystal.Internal;
using Crystal.Multimodal;

namespace Crystal.Generation;

/// <summary>
/// Advertises portable generation support without modeling provider rules.
/// </summary>
public sealed record GenerationCapabilities
{
    /// <summary>Initializes generation capabilities.</summary>
    /// <param name="inputs">The supported individual input shapes.</param>
    /// <param name="outputs">The supported individual output shapes.</param>
    /// <param name="supportsReasoningOptions">
    /// Whether portable reasoning hints are accepted.
    /// </param>
    public GenerationCapabilities(
        IEnumerable<GenerationInputCapability> inputs,
        IEnumerable<MultimodalContentCapability> outputs,
        bool supportsReasoningOptions = false)
    {
        Inputs = CollectionSnapshot.Create(inputs, nameof(inputs));
        Outputs = CollectionSnapshot.Create(
            outputs,
            nameof(outputs),
            allowEmpty: false);

        if (Inputs
            .GroupBy(static input => new
            {
                input.Content.Modality,
                input.Purpose
            })
            .Any(static group => group.Count() != 1))
        {
            throw new ArgumentException(
                "Generation input capability pairs must be unique.",
                nameof(inputs));
        }

        if (Outputs.Select(static output => output.Modality)
            .Distinct()
            .Count() != Outputs.Count)
        {
            throw new ArgumentException(
                "Generation output modalities must be unique.",
                nameof(outputs));
        }

        SupportsReasoningOptions = supportsReasoningOptions;
    }

    /// <summary>Gets supported individual input shapes.</summary>
    public IReadOnlyList<GenerationInputCapability> Inputs { get; }

    /// <summary>Gets supported individual output shapes.</summary>
    public IReadOnlyList<MultimodalContentCapability> Outputs { get; }

    /// <summary>Gets whether portable reasoning hints are accepted.</summary>
    public bool SupportsReasoningOptions { get; }
}
