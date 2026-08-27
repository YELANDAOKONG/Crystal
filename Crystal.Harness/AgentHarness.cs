using Crystal.Agents;
using Crystal.Internal;

namespace Crystal.Harness;

/// <summary>
/// Provides an immutable case-sensitive registry for explicit Agent composition.
/// </summary>
public sealed class AgentHarness
{
    private readonly IReadOnlyDictionary<string, IAgent> _agents;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes an Agent Harness registry.
    /// </summary>
    /// <param name="registrations">The named Agent registrations.</param>
    /// <param name="timeProvider">
    /// An optional time source used for Harness session duration accounting.
    /// </param>
    public AgentHarness(
        IEnumerable<AgentRegistration> registrations,
        TimeProvider? timeProvider = null)
    {
        var snapshot = CollectionSnapshot.Create(
            registrations,
            nameof(registrations));
        var agents = new Dictionary<string, IAgent>(
            snapshot.Count,
            StringComparer.Ordinal);
        var names = new AgentName[snapshot.Count];

        for (var index = 0; index < snapshot.Count; index++)
        {
            var registration = snapshot[index];

            if (!agents.TryAdd(
                    registration.Name.Value,
                    registration.Agent))
            {
                throw new ArgumentException(
                    "Agent names must be unique.",
                    nameof(registrations));
            }

            names[index] = registration.Name;
        }

        _agents = agents;
        _timeProvider = timeProvider ?? TimeProvider.System;
        AgentNames = Array.AsReadOnly(names);
    }

    /// <summary>
    /// Gets registered Agent names in registration order.
    /// </summary>
    public IReadOnlyList<AgentName> AgentNames { get; }

    /// <summary>
    /// Creates an independent bounded Harness session.
    /// </summary>
    /// <param name="sessionId">The caller-supplied non-empty session identifier.</param>
    /// <param name="limits">The finite shared session limits.</param>
    /// <param name="sessionCancellationToken">
    /// A token shared by every invocation in the session.
    /// </param>
    /// <returns>The new Harness session.</returns>
    public IAgentHarnessSession CreateSession(
        Guid sessionId,
        HarnessLimits limits,
        CancellationToken sessionCancellationToken = default)
    {
        if (sessionId == Guid.Empty)
        {
            throw new ArgumentException(
                "Session identifier cannot be empty.",
                nameof(sessionId));
        }

        ArgumentNullException.ThrowIfNull(limits, nameof(limits));

        return new AgentHarnessSession(
            sessionId,
            limits,
            _agents,
            _timeProvider,
            sessionCancellationToken);
    }
}
