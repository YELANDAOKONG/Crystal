using Crystal.Internal;
using Crystal.Tools;

namespace Crystal.Agents;

/// <summary>
/// Records exact ordered tool results after a batch completes.
/// </summary>
public sealed record AgentToolExecutionCompletedEvent : AgentRunEvent
{
    /// <summary>
    /// Initializes a tool-execution-completed event.
    /// </summary>
    /// <param name="runId">The run identifier.</param>
    /// <param name="sequence">The zero-based event sequence.</param>
    /// <param name="modelCallNumber">The one-based model-call number.</param>
    /// <param name="results">The ordered correlated tool results.</param>
    public AgentToolExecutionCompletedEvent(
        Guid runId,
        long sequence,
        int modelCallNumber,
        IEnumerable<ToolResult> results)
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

    /// <summary>
    /// Gets the one-based model-call number.
    /// </summary>
    public int ModelCallNumber { get; }

    /// <summary>
    /// Gets the ordered correlated tool results.
    /// </summary>
    public IReadOnlyList<ToolResult> Results { get; }
}
