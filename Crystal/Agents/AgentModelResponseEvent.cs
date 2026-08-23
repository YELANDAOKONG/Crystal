using Crystal.Chat;

namespace Crystal.Agents;

/// <summary>
/// Records the exact response returned for one Agent model call.
/// </summary>
public sealed record AgentModelResponseEvent : AgentRunEvent
{
    /// <summary>
    /// Initializes a model-response event.
    /// </summary>
    /// <param name="runId">The run identifier.</param>
    /// <param name="sequence">The zero-based event sequence.</param>
    /// <param name="modelCallNumber">The one-based model-call number.</param>
    /// <param name="response">The exact chat-client response.</param>
    public AgentModelResponseEvent(
        Guid runId,
        long sequence,
        int modelCallNumber,
        ChatResponse response)
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

    /// <summary>
    /// Gets the one-based model-call number.
    /// </summary>
    public int ModelCallNumber { get; }

    /// <summary>
    /// Gets the exact model response.
    /// </summary>
    public ChatResponse Response { get; }
}
