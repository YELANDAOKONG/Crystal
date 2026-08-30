using Crystal.Multimodal.Agents;

namespace Crystal.Multimodal.Harness;

/// <summary>Associates a stable name with a multimodal Agent.</summary>
public sealed record MultimodalAgentRegistration
{
    /// <summary>Initializes a multimodal Agent registration.</summary>
    /// <param name="name">The stable case-sensitive name.</param>
    /// <param name="agent">The multimodal Agent implementation.</param>
    public MultimodalAgentRegistration(
        MultimodalAgentName name,
        IMultimodalAgent agent)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));
        ArgumentNullException.ThrowIfNull(agent, nameof(agent));

        Name = name;
        Agent = agent;
    }

    /// <summary>Gets the registered name.</summary>
    public MultimodalAgentName Name { get; }

    /// <summary>Gets the multimodal Agent implementation.</summary>
    public IMultimodalAgent Agent { get; }
}
