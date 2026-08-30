using Crystal.Internal;
using Crystal.Multimodal.Agents;

namespace Crystal.Multimodal.Harness;

/// <summary>
/// Provides an immutable registry for explicit multimodal Agent composition.
/// </summary>
public sealed class MultimodalAgentHarness
{
    private readonly IReadOnlyDictionary<string, IMultimodalAgent> _agents;
    private readonly TimeProvider _timeProvider;

    /// <summary>Initializes a multimodal Agent Harness registry.</summary>
    /// <param name="registrations">The named Agent registrations.</param>
    /// <param name="timeProvider">
    /// An optional time source for session duration accounting.
    /// </param>
    public MultimodalAgentHarness(
        IEnumerable<MultimodalAgentRegistration> registrations,
        TimeProvider? timeProvider = null)
    {
        var snapshot = CollectionSnapshot.Create(
            registrations,
            nameof(registrations));
        var agents = new Dictionary<string, IMultimodalAgent>(
            snapshot.Count,
            StringComparer.Ordinal);
        var names = new MultimodalAgentName[snapshot.Count];

        for (var index = 0; index < snapshot.Count; index++)
        {
            var registration = snapshot[index];

            if (!agents.TryAdd(
                    registration.Name.Value,
                    registration.Agent))
            {
                throw new ArgumentException(
                    "Multimodal Agent names must be unique.",
                    nameof(registrations));
            }

            names[index] = registration.Name;
        }

        _agents = agents;
        _timeProvider = timeProvider ?? TimeProvider.System;
        AgentNames = Array.AsReadOnly(names);
    }

    /// <summary>Gets registered Agent names in registration order.</summary>
    public IReadOnlyList<MultimodalAgentName> AgentNames { get; }

    /// <summary>Creates an independent bounded multimodal Harness session.</summary>
    /// <param name="sessionId">The caller-supplied session identifier.</param>
    /// <param name="limits">The finite shared session limits.</param>
    /// <param name="sessionCancellationToken">
    /// A token shared by every invocation in the session.
    /// </param>
    /// <returns>The new multimodal Harness session.</returns>
    public IMultimodalAgentHarnessSession CreateSession(
        Guid sessionId,
        MultimodalHarnessLimits limits,
        CancellationToken sessionCancellationToken = default)
    {
        if (sessionId == Guid.Empty)
        {
            throw new ArgumentException(
                "Session identifier cannot be empty.",
                nameof(sessionId));
        }

        ArgumentNullException.ThrowIfNull(limits, nameof(limits));

        return new MultimodalAgentHarnessSession(
            sessionId,
            limits,
            _agents,
            _timeProvider,
            sessionCancellationToken);
    }
}
