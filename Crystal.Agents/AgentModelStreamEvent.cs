using Crystal.Chat;

namespace Crystal.Agents;

/// <summary>Forwards one exact event from a streaming text model call.</summary>
public sealed record AgentModelStreamEvent : AgentRunEvent
{
    /// <summary>Initializes a forwarded model-stream event.</summary>
    /// <param name="runId">The run identifier.</param>
    /// <param name="sequence">The zero-based Agent event sequence.</param>
    /// <param name="modelCallNumber">The one-based model-call number.</param>
    /// <param name="streamEvent">The exact client stream event.</param>
    public AgentModelStreamEvent(
        Guid runId,
        long sequence,
        int modelCallNumber,
        ChatStreamEvent streamEvent)
        : base(runId, sequence)
    {
        if (modelCallNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(modelCallNumber),
                modelCallNumber,
                "Model call number must be positive.");
        }

        ArgumentNullException.ThrowIfNull(streamEvent, nameof(streamEvent));

        ModelCallNumber = modelCallNumber;
        StreamEvent = streamEvent;
    }

    /// <summary>Gets the one-based model-call number.</summary>
    public int ModelCallNumber { get; }

    /// <summary>Gets the exact client stream event.</summary>
    public ChatStreamEvent StreamEvent { get; }
}
