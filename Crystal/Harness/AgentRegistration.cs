using Crystal.Agents;

namespace Crystal.Harness;

/// <summary>
/// Associates a stable Harness name with one Agent implementation.
/// </summary>
public sealed record AgentRegistration
{
    /// <summary>
    /// Initializes an Agent registration.
    /// </summary>
    /// <param name="name">The stable case-sensitive name.</param>
    /// <param name="agent">The Agent implementation.</param>
    public AgentRegistration(
        AgentName name,
        IAgent agent)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));
        ArgumentNullException.ThrowIfNull(agent, nameof(agent));

        Name = name;
        Agent = agent;
    }

    /// <summary>
    /// Gets the registered name.
    /// </summary>
    public AgentName Name { get; }

    /// <summary>
    /// Gets the Agent implementation.
    /// </summary>
    public IAgent Agent { get; }
}
