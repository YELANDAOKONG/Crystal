using Crystal.Internal;
using Crystal.Multimodal.Agents;
using Crystal.Multimodal.Chat;
using Crystal.Reasoning;

namespace Crystal.Multimodal.Harness;

/// <summary>Contains one explicit multimodal Agent invocation.</summary>
public sealed record MultimodalAgentInvocationRequest
{
    /// <summary>Initializes a multimodal Agent invocation request.</summary>
    /// <param name="invocationId">
    /// The caller-supplied non-empty invocation identifier.
    /// </param>
    /// <param name="agentName">The registered multimodal Agent name.</param>
    /// <param name="items">The exact ordered initial transcript.</param>
    /// <param name="limits">The requested per-Agent finite limits.</param>
    /// <param name="parentInvocationId">
    /// The optional parent invocation in the same session.
    /// </param>
    /// <param name="reasoning">Optional portable reasoning hints.</param>
    public MultimodalAgentInvocationRequest(
        Guid invocationId,
        MultimodalAgentName agentName,
        IEnumerable<MultimodalChatItem> items,
        MultimodalAgentRunLimits limits,
        Guid? parentInvocationId = null,
        ReasoningOptions? reasoning = null)
    {
        if (invocationId == Guid.Empty)
        {
            throw new ArgumentException(
                "Invocation identifier cannot be empty.",
                nameof(invocationId));
        }

        ArgumentNullException.ThrowIfNull(agentName, nameof(agentName));
        ArgumentNullException.ThrowIfNull(limits, nameof(limits));

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

        InvocationId = invocationId;
        AgentName = agentName;
        Items = CollectionSnapshot.Create(items, nameof(items));
        Limits = limits;
        ParentInvocationId = parentInvocationId;
        Reasoning = reasoning;
    }

    /// <summary>Gets the invocation identifier.</summary>
    public Guid InvocationId { get; }

    /// <summary>Gets the registered multimodal Agent name.</summary>
    public MultimodalAgentName AgentName { get; }

    /// <summary>Gets the exact ordered initial transcript.</summary>
    public IReadOnlyList<MultimodalChatItem> Items { get; }

    /// <summary>Gets the requested per-Agent limits.</summary>
    public MultimodalAgentRunLimits Limits { get; }

    /// <summary>Gets the optional parent invocation identifier.</summary>
    public Guid? ParentInvocationId { get; }

    /// <summary>Gets optional portable reasoning hints.</summary>
    public ReasoningOptions? Reasoning { get; }
}
