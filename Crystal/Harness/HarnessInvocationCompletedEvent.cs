namespace Crystal.Harness;

/// <summary>
/// Marks one Harness invocation stream as complete.
/// </summary>
public sealed record HarnessInvocationCompletedEvent : HarnessEvent
{
    /// <summary>
    /// Initializes a Harness invocation-completed event.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="invocationId">The invocation identifier.</param>
    /// <param name="agentName">The registered Agent name.</param>
    /// <param name="parentInvocationId">The optional parent identifier.</param>
    /// <param name="sequence">The zero-based event sequence.</param>
    /// <param name="result">The final Harness invocation result.</param>
    public HarnessInvocationCompletedEvent(
        Guid sessionId,
        Guid invocationId,
        AgentName agentName,
        Guid? parentInvocationId,
        long sequence,
        AgentInvocationResult result)
        : base(
            sessionId,
            invocationId,
            agentName,
            parentInvocationId,
            sequence)
    {
        ArgumentNullException.ThrowIfNull(result, nameof(result));

        if (result.SessionId != sessionId
            || result.InvocationId != invocationId)
        {
            throw new ArgumentException(
                "The result correlation does not match the Harness event.",
                nameof(result));
        }

        Result = result;
    }

    /// <summary>
    /// Gets the final Harness invocation result.
    /// </summary>
    public AgentInvocationResult Result { get; }
}
