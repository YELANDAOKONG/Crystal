using Crystal.Multimodal.Agents;

namespace Crystal.Multimodal.Harness;

/// <summary>Records effective limits for a started multimodal invocation.</summary>
public sealed record MultimodalHarnessInvocationStartedEvent
    : MultimodalHarnessEvent
{
    /// <summary>Initializes an invocation-started event.</summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="invocationId">The invocation identifier.</param>
    /// <param name="agentName">The registered Agent name.</param>
    /// <param name="parentInvocationId">The optional parent identifier.</param>
    /// <param name="sequence">The zero-based event sequence.</param>
    /// <param name="effectiveLimits">The reserved effective limits.</param>
    public MultimodalHarnessInvocationStartedEvent(
        Guid sessionId,
        Guid invocationId,
        MultimodalAgentName agentName,
        Guid? parentInvocationId,
        long sequence,
        MultimodalAgentRunLimits effectiveLimits)
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

    /// <summary>Gets the reserved effective Agent limits.</summary>
    public MultimodalAgentRunLimits EffectiveLimits { get; }
}
