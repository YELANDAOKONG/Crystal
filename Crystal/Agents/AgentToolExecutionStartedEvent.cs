using Crystal.Internal;
using Crystal.Tools;

namespace Crystal.Agents;

/// <summary>
/// Records an ordered tool-call batch before execution begins.
/// </summary>
public sealed record AgentToolExecutionStartedEvent : AgentRunEvent
{
    /// <summary>
    /// Initializes a tool-execution-started event.
    /// </summary>
    /// <param name="runId">The run identifier.</param>
    /// <param name="sequence">The zero-based event sequence.</param>
    /// <param name="modelCallNumber">The one-based model-call number.</param>
    /// <param name="calls">The ordered tool calls.</param>
    public AgentToolExecutionStartedEvent(
        Guid runId,
        long sequence,
        int modelCallNumber,
        IEnumerable<ToolCall> calls)
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
        Calls = CollectionSnapshot.Create(
            calls,
            nameof(calls),
            allowEmpty: false);
    }

    /// <summary>
    /// Gets the one-based model-call number.
    /// </summary>
    public int ModelCallNumber { get; }

    /// <summary>
    /// Gets the ordered tool calls.
    /// </summary>
    public IReadOnlyList<ToolCall> Calls { get; }
}
