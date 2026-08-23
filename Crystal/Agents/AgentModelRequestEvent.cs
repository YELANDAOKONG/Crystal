using Crystal.Chat;

namespace Crystal.Agents;

/// <summary>
/// Records the exact request sent for one Agent model call.
/// </summary>
public sealed record AgentModelRequestEvent : AgentRunEvent
{
    /// <summary>
    /// Initializes a model-request event.
    /// </summary>
    /// <param name="runId">The run identifier.</param>
    /// <param name="sequence">The zero-based event sequence.</param>
    /// <param name="modelCallNumber">The one-based model-call number.</param>
    /// <param name="request">The exact request sent to the chat client.</param>
    public AgentModelRequestEvent(
        Guid runId,
        long sequence,
        int modelCallNumber,
        ChatRequest request)
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

    /// <summary>
    /// Gets the one-based model-call number.
    /// </summary>
    public int ModelCallNumber { get; }

    /// <summary>
    /// Gets the exact model request.
    /// </summary>
    public ChatRequest Request { get; }
}
