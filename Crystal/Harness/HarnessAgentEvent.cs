using Crystal.Agents;

namespace Crystal.Harness;

/// <summary>
/// Wraps one exact Agent event with Harness invocation ancestry.
/// </summary>
public sealed record HarnessAgentEvent : HarnessEvent
{
    /// <summary>
    /// Initializes a forwarded Agent event.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="invocationId">The invocation identifier.</param>
    /// <param name="agentName">The registered Agent name.</param>
    /// <param name="parentInvocationId">The optional parent identifier.</param>
    /// <param name="sequence">The zero-based Harness event sequence.</param>
    /// <param name="agentEvent">The exact Agent event.</param>
    public HarnessAgentEvent(
        Guid sessionId,
        Guid invocationId,
        AgentName agentName,
        Guid? parentInvocationId,
        long sequence,
        AgentRunEvent agentEvent)
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

    /// <summary>
    /// Gets the exact forwarded Agent event.
    /// </summary>
    public AgentRunEvent AgentEvent { get; }
}
