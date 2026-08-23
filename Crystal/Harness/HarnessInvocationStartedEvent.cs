using Crystal.Agents;

namespace Crystal.Harness;

/// <summary>
/// Records the effective limits reserved for a started Agent invocation.
/// </summary>
public sealed record HarnessInvocationStartedEvent : HarnessEvent
{
    /// <summary>
    /// Initializes a Harness invocation-started event.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="invocationId">The invocation identifier.</param>
    /// <param name="agentName">The registered Agent name.</param>
    /// <param name="parentInvocationId">The optional parent identifier.</param>
    /// <param name="sequence">The zero-based event sequence.</param>
    /// <param name="effectiveLimits">The reserved effective Agent limits.</param>
    public HarnessInvocationStartedEvent(
        Guid sessionId,
        Guid invocationId,
        AgentName agentName,
        Guid? parentInvocationId,
        long sequence,
        AgentRunLimits effectiveLimits)
        : base(
            sessionId,
            invocationId,
            agentName,
            parentInvocationId,
            sequence)
    {
        ArgumentNullException.ThrowIfNull(
            effectiveLimits,
            nameof(effectiveLimits));
        EffectiveLimits = effectiveLimits;
    }

    /// <summary>
    /// Gets the reserved effective Agent limits.
    /// </summary>
    public AgentRunLimits EffectiveLimits { get; }
}
