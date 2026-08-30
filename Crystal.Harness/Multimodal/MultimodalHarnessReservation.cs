using Crystal.Multimodal.Agents;

namespace Crystal.Multimodal.Harness;

internal sealed class MultimodalHarnessReservation
{
    private MultimodalHarnessReservation(
        IMultimodalAgent? agent,
        MultimodalAgentRunLimits? effectiveLimits,
        MultimodalAgentInvocationOutcome? deniedOutcome,
        int reservedModelCalls,
        int reservedToolCalls)
    {
        Agent = agent;
        EffectiveLimits = effectiveLimits;
        DeniedOutcome = deniedOutcome;
        ReservedModelCalls = reservedModelCalls;
        ReservedToolCalls = reservedToolCalls;
    }

    public IMultimodalAgent? Agent { get; }

    public MultimodalAgentRunLimits? EffectiveLimits { get; }

    public MultimodalAgentInvocationOutcome? DeniedOutcome { get; }

    public int ReservedModelCalls { get; }

    public int ReservedToolCalls { get; }

    public bool IsGranted => Agent is not null;

    public static MultimodalHarnessReservation Granted(
        IMultimodalAgent agent,
        MultimodalAgentRunLimits effectiveLimits) =>
        new(
            agent,
            effectiveLimits,
            null,
            effectiveLimits.MaximumModelCalls,
            effectiveLimits.MaximumToolCalls);

    public static MultimodalHarnessReservation Denied(
        MultimodalAgentInvocationOutcome outcome) =>
        new(null, null, outcome, 0, 0);
}
