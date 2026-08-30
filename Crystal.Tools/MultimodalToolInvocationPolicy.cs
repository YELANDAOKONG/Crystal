namespace Crystal.Multimodal.Tools;

/// <summary>Evaluates one multimodal tool call before invocation.</summary>
/// <param name="call">The exact model-generated tool call.</param>
/// <param name="cancellationToken">A token that cancels policy work.</param>
/// <returns>The caller-owned invocation decision.</returns>
public delegate ValueTask<MultimodalToolInvocationDecision>
    MultimodalToolInvocationPolicy(
        MultimodalToolCall call,
        CancellationToken cancellationToken);
