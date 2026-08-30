using Crystal.Multimodal.Chat;

namespace Crystal.Multimodal.Agents;

/// <summary>Records the exact request for one multimodal model call.</summary>
public sealed record MultimodalAgentModelRequestEvent
    : MultimodalAgentRunEvent
{
    /// <summary>Initializes a multimodal model-request event.</summary>
    /// <param name="runId">The run identifier.</param>
    /// <param name="sequence">The zero-based event sequence.</param>
    /// <param name="modelCallNumber">The one-based model-call number.</param>
    /// <param name="request">The exact request sent to the client.</param>
    public MultimodalAgentModelRequestEvent(
        Guid runId,
        long sequence,
        int modelCallNumber,
        MultimodalChatRequest request)
        : base(runId, sequence)
    {
        if (modelCallNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(modelCallNumber),
                modelCallNumber,
                "Model call number must be positive.");
        }

        ArgumentNullException.ThrowIfNull(request, nameof(request));

        ModelCallNumber = modelCallNumber;
        Request = request;
    }

    /// <summary>Gets the one-based model-call number.</summary>
    public int ModelCallNumber { get; }

    /// <summary>Gets the exact model request.</summary>
    public MultimodalChatRequest Request { get; }
}
