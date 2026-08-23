namespace Crystal.Tools;

/// <summary>
/// Evaluates one model-generated tool call before invocation.
/// </summary>
/// <param name="call">The exact model-generated tool call.</param>
/// <param name="cancellationToken">
/// A token that cancels policy evaluation.
/// </param>
/// <returns>The caller-owned invocation decision.</returns>
public delegate ValueTask<ToolInvocationDecision> ToolInvocationPolicy(
    ToolCall call,
    CancellationToken cancellationToken);
