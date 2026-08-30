using Crystal.Internal;

namespace Crystal.Multimodal.Chat;

/// <summary>
/// Advertises portable multimodal Chat support without encoding model rules.
/// </summary>
public sealed record MultimodalChatCapabilities
{
    /// <summary>Initializes multimodal Chat capabilities.</summary>
    /// <param name="inputs">The supported individual input shapes.</param>
    /// <param name="outputs">The supported individual output shapes.</param>
    /// <param name="supportsTools">Whether caller-defined tools are accepted.</param>
    /// <param name="supportsReasoningOptions">
    /// Whether portable reasoning hints are accepted.
    /// </param>
    public MultimodalChatCapabilities(
        IEnumerable<MultimodalContentCapability> inputs,
        IEnumerable<MultimodalContentCapability> outputs,
        bool supportsTools = false,
        bool supportsReasoningOptions = false)
    {
        Inputs = CollectionSnapshot.Create(
            inputs,
            nameof(inputs),
            allowEmpty: false);
        Outputs = CollectionSnapshot.Create(
            outputs,
            nameof(outputs),
            allowEmpty: false);

        if (Inputs.Select(static input => input.Modality)
            .Distinct()
            .Count() != Inputs.Count)
        {
            throw new ArgumentException(
                "Multimodal Chat input modalities must be unique.",
                nameof(inputs));
        }

        if (Outputs.Select(static output => output.Modality)
            .Distinct()
            .Count() != Outputs.Count)
        {
            throw new ArgumentException(
                "Multimodal Chat output modalities must be unique.",
                nameof(outputs));
        }

        SupportsTools = supportsTools;
        SupportsReasoningOptions = supportsReasoningOptions;
    }

    /// <summary>Gets supported individual input shapes.</summary>
    public IReadOnlyList<MultimodalContentCapability> Inputs { get; }

    /// <summary>Gets supported individual output shapes.</summary>
    public IReadOnlyList<MultimodalContentCapability> Outputs { get; }

    /// <summary>Gets whether caller-defined tools are accepted.</summary>
    public bool SupportsTools { get; }

    /// <summary>Gets whether portable reasoning hints are accepted.</summary>
    public bool SupportsReasoningOptions { get; }
}
