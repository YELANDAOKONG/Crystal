using Crystal.Internal;
using Crystal.Multimodal;

namespace Crystal.Realtime;

/// <summary>Advertises portable individual live-session content shapes.</summary>
public sealed record RealtimeSessionCapabilities
{
    /// <summary>Initializes live-session capabilities.</summary>
    public RealtimeSessionCapabilities(
        IEnumerable<MultimodalContentCapability> inputs,
        IEnumerable<MultimodalContentCapability> outputs,
        bool supportsAutomaticTurnDetection,
        bool supportsExplicitTurnEnd,
        bool supportsTools = false,
        bool supportsReasoningOptions = false)
    {
        Inputs = CollectionSnapshot.Create(inputs, nameof(inputs), allowEmpty: false);
        Outputs = CollectionSnapshot.Create(outputs, nameof(outputs), allowEmpty: false);

        if (Inputs.Select(static input => input.Modality).Distinct().Count() != Inputs.Count)
        {
            throw new ArgumentException("Input modalities must be unique.", nameof(inputs));
        }

        if (Outputs.Select(static output => output.Modality).Distinct().Count() != Outputs.Count)
        {
            throw new ArgumentException("Output modalities must be unique.", nameof(outputs));
        }

        if (!supportsAutomaticTurnDetection && !supportsExplicitTurnEnd)
        {
            throw new ArgumentException("At least one turn mode must be supported.");
        }

        SupportsAutomaticTurnDetection = supportsAutomaticTurnDetection;
        SupportsExplicitTurnEnd = supportsExplicitTurnEnd;
        SupportsTools = supportsTools;
        SupportsReasoningOptions = supportsReasoningOptions;
    }

    /// <summary>Gets supported individual input shapes.</summary>
    public IReadOnlyList<MultimodalContentCapability> Inputs { get; }

    /// <summary>Gets supported individual output shapes.</summary>
    public IReadOnlyList<MultimodalContentCapability> Outputs { get; }

    /// <summary>Gets whether the remote service can detect turn boundaries.</summary>
    public bool SupportsAutomaticTurnDetection { get; }

    /// <summary>Gets whether the caller can signal a turn boundary.</summary>
    public bool SupportsExplicitTurnEnd { get; }

    /// <summary>Gets whether caller-authored tool definitions are supported.</summary>
    public bool SupportsTools { get; }

    /// <summary>Gets whether portable reasoning hints are supported.</summary>
    public bool SupportsReasoningOptions { get; }
}
