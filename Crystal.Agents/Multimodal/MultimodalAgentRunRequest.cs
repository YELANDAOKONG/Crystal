using Crystal.Internal;
using Crystal.Multimodal.Chat;
using Crystal.Reasoning;

namespace Crystal.Multimodal.Agents;

/// <summary>Contains one caller-authored multimodal Agent run request.</summary>
public sealed record MultimodalAgentRunRequest
{
    /// <summary>Initializes a multimodal Agent run request.</summary>
    /// <param name="runId">The caller-supplied non-empty run identifier.</param>
    /// <param name="items">The exact ordered initial transcript.</param>
    /// <param name="limits">The finite run limits.</param>
    /// <param name="reasoning">Optional portable reasoning hints.</param>
    public MultimodalAgentRunRequest(
        Guid runId,
        IEnumerable<MultimodalChatItem> items,
        MultimodalAgentRunLimits limits,
        ReasoningOptions? reasoning = null)
    {
        if (runId == Guid.Empty)
        {
            throw new ArgumentException(
                "Run identifier cannot be empty.",
                nameof(runId));
        }

        ArgumentNullException.ThrowIfNull(limits, nameof(limits));

        RunId = runId;
        Items = CollectionSnapshot.Create(items, nameof(items));
        Limits = limits;
        Reasoning = reasoning;
    }

    /// <summary>Gets the caller-supplied run identifier.</summary>
    public Guid RunId { get; }

    /// <summary>
    /// Gets the exact ordered initial transcript. Every referenced media source
    /// must remain valid and replayable for the run duration.
    /// </summary>
    public IReadOnlyList<MultimodalChatItem> Items { get; }

    /// <summary>Gets the finite run limits.</summary>
    public MultimodalAgentRunLimits Limits { get; }

    /// <summary>Gets optional portable reasoning hints.</summary>
    public ReasoningOptions? Reasoning { get; }
}
