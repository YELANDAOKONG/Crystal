using Crystal.Internal;
using Crystal.Multimodal.Chat;

namespace Crystal.Multimodal.Agents;

/// <summary>Contains the exact transcript and accounting for a completed run.</summary>
public sealed record MultimodalAgentRunResult
{
    /// <summary>Initializes a multimodal Agent run result.</summary>
    /// <param name="runId">The non-empty run identifier.</param>
    /// <param name="transcript">The exact ordered final transcript.</param>
    /// <param name="stopReason">The reason the run stopped.</param>
    /// <param name="modelCallCount">The attempted model-call count.</param>
    /// <param name="toolCallCount">The attempted tool-call count.</param>
    /// <param name="usage">
    /// Aggregated usage when every attempted model call reported it.
    /// </param>
    /// <param name="finalFinishReason">
    /// The terminal candidate finish reason for normal completion.
    /// </param>
    public MultimodalAgentRunResult(
        Guid runId,
        IEnumerable<MultimodalChatItem> transcript,
        MultimodalAgentRunStopReason stopReason,
        int modelCallCount,
        int toolCallCount,
        TokenUsage? usage = null,
        FinishReason? finalFinishReason = null)
    {
        if (runId == Guid.Empty)
        {
            throw new ArgumentException(
                "Run identifier cannot be empty.",
                nameof(runId));
        }

        ArgumentNullException.ThrowIfNull(stopReason, nameof(stopReason));

        if (modelCallCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(modelCallCount),
                modelCallCount,
                "Model call count cannot be negative.");
        }

        if (toolCallCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(toolCallCount),
                toolCallCount,
                "Tool call count cannot be negative.");
        }

        if (stopReason == MultimodalAgentRunStopReason.Completed
            && finalFinishReason is null)
        {
            throw new ArgumentException(
                "A completed run requires a final finish reason.",
                nameof(finalFinishReason));
        }

        if (stopReason != MultimodalAgentRunStopReason.Completed
            && finalFinishReason is not null)
        {
            throw new ArgumentException(
                "A limit-stopped run cannot have a final finish reason.",
                nameof(finalFinishReason));
        }

        RunId = runId;
        Transcript = CollectionSnapshot.Create(transcript, nameof(transcript));
        StopReason = stopReason;
        ModelCallCount = modelCallCount;
        ToolCallCount = toolCallCount;
        Usage = usage;
        FinalFinishReason = finalFinishReason;
    }

    /// <summary>Gets the run identifier.</summary>
    public Guid RunId { get; }

    /// <summary>Gets the exact ordered final transcript.</summary>
    public IReadOnlyList<MultimodalChatItem> Transcript { get; }

    /// <summary>Gets the reason the run stopped.</summary>
    public MultimodalAgentRunStopReason StopReason { get; }

    /// <summary>Gets the attempted model-call count.</summary>
    public int ModelCallCount { get; }

    /// <summary>Gets the attempted tool-call count.</summary>
    public int ToolCallCount { get; }

    /// <summary>Gets aggregated usage when every model call reported it.</summary>
    public TokenUsage? Usage { get; }

    /// <summary>Gets the terminal finish reason for normal completion.</summary>
    public FinishReason? FinalFinishReason { get; }
}
