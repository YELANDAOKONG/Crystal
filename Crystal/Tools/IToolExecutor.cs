namespace Crystal.Tools;

/// <summary>
/// Defines explicit execution for ordered model tool-call batches.
/// </summary>
public interface IToolExecutor
{
    /// <summary>
    /// Gets the model-facing definitions in registration order.
    /// </summary>
    IReadOnlyList<ToolDefinition> Definitions { get; }

    /// <summary>
    /// Executes a batch and returns results in input call order.
    /// </summary>
    /// <param name="calls">The ordered complete tool calls.</param>
    /// <param name="cancellationToken">
    /// A token that cancels policy and tool operations.
    /// </param>
    /// <returns>The ordered correlated tool results.</returns>
    Task<IReadOnlyList<ToolResult>> ExecuteAsync(
        IEnumerable<ToolCall> calls,
        CancellationToken cancellationToken = default);
}
