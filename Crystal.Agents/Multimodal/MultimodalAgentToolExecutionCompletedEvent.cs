using Crystal.Internal;
using Crystal.Multimodal.Tools;

namespace Crystal.Multimodal.Agents;

/// <summary>Records exact multimodal results after tool execution.</summary>
public sealed record MultimodalAgentToolExecutionCompletedEvent
    : MultimodalAgentRunEvent
{
    /// <summary>Initializes a tool-execution-completed event.</summary>
    /// <param name="runId">The run identifier.</param>
    /// <param name="sequence">The zero-based event sequence.</param>
    /// <param name="modelCallNumber">The one-based model-call number.</param>
    /// <param name="results">The non-empty ordered correlated results.</param>
    public MultimodalAgentToolExecutionCompletedEvent(
        Guid runId,
        long sequence,
        int modelCallNumber,
        IEnumerable<MultimodalToolResult> results)
        : base(runId, sequence)
    {
        if (modelCallNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(modelCallNumber),
                modelCallNumber,
                "Model call number must be positive.");
        }

        ModelCallNumber = modelCallNumber;
        Results = CollectionSnapshot.Create(
            results,
            nameof(results),
            allowEmpty: false);
    }

    /// <summary>Gets the one-based model-call number.</summary>
    public int ModelCallNumber { get; }

    /// <summary>Gets the ordered correlated multimodal results.</summary>
    public IReadOnlyList<MultimodalToolResult> Results { get; }
}
