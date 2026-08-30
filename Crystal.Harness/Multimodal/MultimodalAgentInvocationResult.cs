using Crystal.Multimodal.Agents;

namespace Crystal.Multimodal.Harness;

/// <summary>Contains the Harness outcome for a multimodal Agent invocation.</summary>
public sealed record MultimodalAgentInvocationResult
{
    /// <summary>Initializes a multimodal Agent invocation result.</summary>
    /// <param name="sessionId">The non-empty session identifier.</param>
    /// <param name="invocationId">The non-empty invocation identifier.</param>
    /// <param name="agentName">The registered multimodal Agent name.</param>
    /// <param name="outcome">The Harness invocation outcome.</param>
    /// <param name="parentInvocationId">The optional parent identifier.</param>
    /// <param name="agentResult">
    /// The Agent result when the Harness started the invocation.
    /// </param>
    public MultimodalAgentInvocationResult(
        Guid sessionId,
        Guid invocationId,
        MultimodalAgentName agentName,
        MultimodalAgentInvocationOutcome outcome,
        Guid? parentInvocationId = null,
        MultimodalAgentRunResult? agentResult = null)
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
        ArgumentNullException.ThrowIfNull(outcome, nameof(outcome));

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

        if (outcome == MultimodalAgentInvocationOutcome.Completed
            && agentResult is null)
        {
            throw new ArgumentException(
                "A completed invocation requires an Agent result.",
                nameof(agentResult));
        }

        if (outcome != MultimodalAgentInvocationOutcome.Completed
            && agentResult is not null)
        {
            throw new ArgumentException(
                "An invocation that did not start cannot have an Agent result.",
                nameof(agentResult));
        }

        if (agentResult is not null && agentResult.RunId != invocationId)
        {
            throw new ArgumentException(
                "The Agent result run identifier does not match the invocation.",
                nameof(agentResult));
        }

        SessionId = sessionId;
        InvocationId = invocationId;
        AgentName = agentName;
        Outcome = outcome;
        ParentInvocationId = parentInvocationId;
        AgentResult = agentResult;
    }

    /// <summary>Gets the Harness session identifier.</summary>
    public Guid SessionId { get; }

    /// <summary>Gets the invocation identifier.</summary>
    public Guid InvocationId { get; }

    /// <summary>Gets the registered multimodal Agent name.</summary>
    public MultimodalAgentName AgentName { get; }

    /// <summary>Gets the Harness invocation outcome.</summary>
    public MultimodalAgentInvocationOutcome Outcome { get; }

    /// <summary>Gets the optional parent invocation identifier.</summary>
    public Guid? ParentInvocationId { get; }

    /// <summary>Gets the Agent result when the invocation started.</summary>
    public MultimodalAgentRunResult? AgentResult { get; }
}
