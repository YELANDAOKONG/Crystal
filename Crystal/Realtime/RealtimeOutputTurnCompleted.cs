namespace Crystal.Realtime;

/// <summary>Reports completion of one correlated model output turn.</summary>
public sealed record RealtimeOutputTurnCompleted : RealtimeOutputEvent
{
    /// <summary>Initializes one output completion marker.</summary>
    public RealtimeOutputTurnCompleted(
        string outputId,
        FinishReason? finishReason = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputId);
        OutputId = outputId;
        FinishReason = finishReason;
    }

    /// <summary>Gets the stable completed output identifier.</summary>
    public string OutputId { get; }

    /// <summary>Gets the exact reported finish reason when available.</summary>
    public FinishReason? FinishReason { get; }
}
