using Crystal.Multimodal.Chat;

namespace Crystal.Multimodal.Agents;

/// <summary>Records the exact response for one multimodal model call.</summary>
public sealed record MultimodalAgentModelResponseEvent
    : MultimodalAgentRunEvent
{
    /// <summary>Initializes a multimodal model-response event.</summary>
    /// <param name="runId">The run identifier.</param>
    /// <param name="sequence">The zero-based event sequence.</param>
    /// <param name="modelCallNumber">The one-based model-call number.</param>
    /// <param name="response">The exact client response.</param>
    public MultimodalAgentModelResponseEvent(
        Guid runId,
        long sequence,
        int modelCallNumber,
        MultimodalChatResponse response)
        : base(runId, sequence)
    {
        if (modelCallNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(modelCallNumber),
                modelCallNumber,
                "Model call number must be positive.");
        }

        ArgumentNullException.ThrowIfNull(response, nameof(response));

        ModelCallNumber = modelCallNumber;
        Response = response;
    }

    /// <summary>Gets the one-based model-call number.</summary>
    public int ModelCallNumber { get; }

    /// <summary>Gets the exact model response.</summary>
    public MultimodalChatResponse Response { get; }
}
