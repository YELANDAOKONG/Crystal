using Crystal.Internal;
using Crystal.Multimodal.Tools;

namespace Crystal.Multimodal.Agents;

/// <summary>Records a multimodal tool-call batch before execution.</summary>
public sealed record MultimodalAgentToolExecutionStartedEvent
    : MultimodalAgentRunEvent
{
    /// <summary>Initializes a tool-execution-started event.</summary>
    /// <param name="runId">The run identifier.</param>
    /// <param name="sequence">The zero-based event sequence.</param>
    /// <param name="modelCallNumber">The one-based model-call number.</param>
    /// <param name="calls">The non-empty ordered calls.</param>
    public MultimodalAgentToolExecutionStartedEvent(
        Guid runId,
        long sequence,
        int modelCallNumber,
        IEnumerable<MultimodalToolCall> calls)
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

    /// <summary>Gets the one-based model-call number.</summary>
    public int ModelCallNumber { get; }

    /// <summary>Gets the ordered multimodal tool calls.</summary>
    public IReadOnlyList<MultimodalToolCall> Calls { get; }
}
