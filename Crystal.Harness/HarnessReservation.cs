using Crystal.Agents;

namespace Crystal.Harness;

internal sealed class HarnessReservation
{
    private HarnessReservation(
        IAgent? agent,
        AgentRunLimits? effectiveLimits,
        AgentInvocationOutcome? deniedOutcome,
        int reservedModelCalls,
        int reservedToolCalls)
    {
        Agent = agent;
        EffectiveLimits = effectiveLimits;
        DeniedOutcome = deniedOutcome;
        ReservedModelCalls = reservedModelCalls;
        ReservedToolCalls = reservedToolCalls;
    }

    public IAgent? Agent { get; }

    public AgentRunLimits? EffectiveLimits { get; }

    public AgentInvocationOutcome? DeniedOutcome { get; }

    public int ReservedModelCalls { get; }

    public int ReservedToolCalls { get; }

    public bool IsGranted => Agent is not null;

    public static HarnessReservation Granted(
        IAgent agent,
        AgentRunLimits effectiveLimits) =>
        new(
            agent,
            effectiveLimits,
            null,
            effectiveLimits.MaximumModelCalls,
            effectiveLimits.MaximumToolCalls);

    public static HarnessReservation Denied(
        AgentInvocationOutcome outcome) =>
        new(null, null, outcome, 0, 0);
}
