namespace Crystal.Tools;

/// <summary>
/// Defines one caller-owned executable tool.
/// </summary>
public interface ITool
{
    /// <summary>
    /// Gets the caller-authored model-facing definition.
    /// </summary>
    ToolDefinition Definition { get; }

    /// <summary>
    /// Invokes the tool with one complete model call.
    /// </summary>
    /// <param name="call">The exact model-generated tool call.</param>
    /// <param name="cancellationToken">
    /// A token that cancels the tool operation.
    /// </param>
    /// <returns>The exact caller-owned tool output.</returns>
    ValueTask<ToolOutput> InvokeAsync(
        ToolCall call,
        CancellationToken cancellationToken = default);
}
