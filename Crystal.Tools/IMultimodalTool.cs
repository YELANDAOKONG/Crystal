using Crystal.Tools;

namespace Crystal.Multimodal.Tools;

/// <summary>Defines one caller-owned executable multimodal tool.</summary>
public interface IMultimodalTool
{
    /// <summary>Gets the caller-authored model-facing definition.</summary>
    ToolDefinition Definition { get; }

    /// <summary>Invokes the tool with one complete model call.</summary>
    /// <param name="call">The exact model-generated tool call.</param>
    /// <param name="cancellationToken">A token that cancels tool work.</param>
    /// <returns>The exact caller-owned multimodal output.</returns>
    ValueTask<MultimodalToolOutput> InvokeAsync(
        MultimodalToolCall call,
        CancellationToken cancellationToken = default);
}
