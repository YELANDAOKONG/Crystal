using Crystal.Multimodal.Chat;

namespace Crystal.Multimodal.Agents;

/// <summary>Forwards one exact event from a streaming multimodal model call.</summary>
public sealed record MultimodalAgentModelStreamEvent
    : MultimodalAgentRunEvent
{
    /// <summary>Initializes a forwarded model-stream event.</summary>
    /// <param name="runId">The run identifier.</param>
    /// <param name="sequence">The zero-based Agent event sequence.</param>
    /// <param name="modelCallNumber">The one-based model-call number.</param>
    /// <param name="streamEvent">The exact client stream event.</param>
    public MultimodalAgentModelStreamEvent(
        Guid runId,
        long sequence,
        int modelCallNumber,
        MultimodalChatStreamEvent streamEvent)
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
    public MultimodalChatStreamEvent StreamEvent { get; }
}
