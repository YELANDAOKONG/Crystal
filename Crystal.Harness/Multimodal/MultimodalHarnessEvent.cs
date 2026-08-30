namespace Crystal.Multimodal.Harness;

/// <summary>Represents one ordered multimodal Harness invocation event.</summary>
public abstract record MultimodalHarnessEvent
{
    private protected MultimodalHarnessEvent(
        Guid sessionId,
        Guid invocationId,
        MultimodalAgentName agentName,
        Guid? parentInvocationId,
        long sequence)
    {
        if (sessionId == Guid.Empty)
        {
            throw new ArgumentException(
                "Session identifier cannot be empty.",
                nameof(sessionId));
        }

        if (invocationId == Guid.Empty)
        {
            throw new ArgumentException(
                "Invocation identifier cannot be empty.",
                nameof(invocationId));
        }

        ArgumentNullException.ThrowIfNull(agentName, nameof(agentName));

        if (parentInvocationId == Guid.Empty)
        {
            throw new ArgumentException(
                "Parent invocation identifier cannot be empty.",
                nameof(parentInvocationId));
        }

        if (parentInvocationId == invocationId)
        {
            throw new ArgumentException(
                "An invocation cannot be its own parent.",
                nameof(parentInvocationId));
        }

        if (sequence < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sequence),
                sequence,
                "Event sequence cannot be negative.");
        }

        SessionId = sessionId;
        InvocationId = invocationId;
        AgentName = agentName;
        ParentInvocationId = parentInvocationId;
        Sequence = sequence;
    }

    /// <summary>Gets the Harness session identifier.</summary>
    public Guid SessionId { get; }

    /// <summary>Gets the invocation identifier.</summary>
    public Guid InvocationId { get; }

    /// <summary>Gets the registered multimodal Agent name.</summary>
    public MultimodalAgentName AgentName { get; }

    /// <summary>Gets the optional parent invocation identifier.</summary>
    public Guid? ParentInvocationId { get; }

    /// <summary>Gets the zero-based event sequence for this invocation.</summary>
    public long Sequence { get; }
}
