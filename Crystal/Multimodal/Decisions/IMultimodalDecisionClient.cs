using Crystal.Decisions;

namespace Crystal.Multimodal.Decisions;

/// <summary>Evaluates decisions against typed multimodal evidence.</summary>
public interface IMultimodalDecisionClient
{
    /// <summary>Gets the portable question and input shapes this client accepts.</summary>
    MultimodalDecisionCapabilities Capabilities { get; }

    /// <summary>Evaluates the exact ordered questions against the supplied evidence.</summary>
    /// <param name="request">The caller-authored decision request.</param>
    /// <param name="cancellationToken">A token that cancels provider work.</param>
    /// <returns>The ordered results or the whole-request block.</returns>
    Task<DecisionResponse> DecideAsync(
        MultimodalDecisionRequest request,
        CancellationToken cancellationToken = default);
}
