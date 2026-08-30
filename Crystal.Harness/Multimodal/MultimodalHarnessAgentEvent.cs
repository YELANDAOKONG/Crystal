using Crystal.Multimodal.Agents;

namespace Crystal.Multimodal.Harness;

/// <summary>Wraps one exact multimodal Agent event with ancestry.</summary>
public sealed record MultimodalHarnessAgentEvent : MultimodalHarnessEvent
{
    /// <summary>Initializes a forwarded multimodal Agent event.</summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="invocationId">The invocation identifier.</param>
    /// <param name="agentName">The registered Agent name.</param>
    /// <param name="parentInvocationId">The optional parent identifier.</param>
    /// <param name="sequence">The zero-based Harness event sequence.</param>
    /// <param name="agentEvent">The exact multimodal Agent event.</param>
    public MultimodalHarnessAgentEvent(
        Guid sessionId,
        Guid invocationId,
        MultimodalAgentName agentName,
        Guid? parentInvocationId,
        long sequence,
        MultimodalAgentRunEvent agentEvent)
        : base(
            sessionId,
            invocationId,
            agentName,
            parentInvocationId,
            sequence)
    {
        ArgumentNullException.ThrowIfNull(agentEvent, nameof(agentEvent));

        if (agentEvent.RunId != invocationId)
        {
            throw new ArgumentException(
                "The Agent event run identifier does not match the invocation.",
                nameof(agentEvent));
        }

        AgentEvent = agentEvent;
    }

    /// <summary>Gets the exact forwarded multimodal Agent event.</summary>
    public MultimodalAgentRunEvent AgentEvent { get; }
}
