namespace Crystal.Multimodal.Harness;

/// <summary>Marks one multimodal Harness invocation stream complete.</summary>
public sealed record MultimodalHarnessInvocationCompletedEvent
    : MultimodalHarnessEvent
{
    /// <summary>Initializes an invocation-completed event.</summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="invocationId">The invocation identifier.</param>
    /// <param name="agentName">The registered Agent name.</param>
    /// <param name="parentInvocationId">The optional parent identifier.</param>
    /// <param name="sequence">The zero-based event sequence.</param>
    /// <param name="result">The final invocation result.</param>
    public MultimodalHarnessInvocationCompletedEvent(
        Guid sessionId,
        Guid invocationId,
        MultimodalAgentName agentName,
        Guid? parentInvocationId,
        long sequence,
        MultimodalAgentInvocationResult result)
        : base(
            sessionId,
            invocationId,
            agentName,
            parentInvocationId,
            sequence)
    {
        ArgumentNullException.ThrowIfNull(result, nameof(result));

        if (result.SessionId != sessionId
            || result.InvocationId != invocationId
            || result.AgentName != agentName
            || result.ParentInvocationId != parentInvocationId)
        {
            throw new ArgumentException(
                "The result correlation does not match the Harness event.",
                nameof(result));
        }

        Result = result;
    }

    /// <summary>Gets the final invocation result.</summary>
    public MultimodalAgentInvocationResult Result { get; }
}
