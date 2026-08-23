namespace Crystal.Harness;

/// <summary>
/// Defines one bounded Agent Harness session.
/// </summary>
public interface IAgentHarnessSession
{
    /// <summary>
    /// Gets the session identifier.
    /// </summary>
    Guid SessionId { get; }

    /// <summary>
    /// Gets the finite shared session limits.
    /// </summary>
    HarnessLimits Limits { get; }

    /// <summary>
    /// Invokes one registered Agent to a Harness result.
    /// </summary>
    /// <param name="request">The explicit invocation request.</param>
    /// <param name="cancellationToken">
    /// A token that cancels this invocation.
    /// </param>
    /// <returns>The Harness invocation result.</returns>
    Task<AgentInvocationResult> InvokeAsync(
        AgentInvocationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams one explicit Agent invocation and its forwarded Agent events.
    /// </summary>
    /// <param name="request">The explicit invocation request.</param>
    /// <param name="cancellationToken">
    /// A token that cancels enumeration and this invocation.
    /// </param>
    /// <returns>The ordered Harness events.</returns>
    IAsyncEnumerable<HarnessEvent> StreamAsync(
        AgentInvocationRequest request,
        CancellationToken cancellationToken = default);
}
