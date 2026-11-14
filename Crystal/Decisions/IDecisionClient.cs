namespace Crystal.Decisions;

/// <summary>Evaluates decisions against text or structured JSON without media.</summary>
public interface IDecisionClient
{
    /// <summary>Gets the portable question and input shapes this client accepts.</summary>
    DecisionCapabilities Capabilities { get; }

    /// <summary>Evaluates the exact ordered questions against the supplied evidence.</summary>
    /// <param name="request">The caller-authored decision request.</param>
    /// <param name="cancellationToken">A token that cancels provider work.</param>
    /// <returns>The ordered results or the whole-request block.</returns>
    Task<DecisionResponse> DecideAsync(
        DecisionRequest request,
        CancellationToken cancellationToken = default);
}
