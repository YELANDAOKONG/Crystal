using Crystal.Tools;

namespace Crystal.Multimodal.Tools;

/// <summary>Defines explicit execution for multimodal tool-call batches.</summary>
public interface IMultimodalToolExecutor
{
    /// <summary>Gets model-facing definitions in registration order.</summary>
    IReadOnlyList<ToolDefinition> Definitions { get; }

    /// <summary>Executes calls and preserves their input order.</summary>
    /// <param name="calls">The ordered complete multimodal tool calls.</param>
    /// <param name="cancellationToken">
    /// A token that cancels policy and tool work.
    /// </param>
    /// <returns>The ordered correlated multimodal results.</returns>
    Task<IReadOnlyList<MultimodalToolResult>> ExecuteAsync(
        IEnumerable<MultimodalToolCall> calls,
        CancellationToken cancellationToken = default);
}
