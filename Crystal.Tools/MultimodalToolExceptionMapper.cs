namespace Crystal.Multimodal.Tools;

/// <summary>Optionally maps a tool exception to caller-owned output.</summary>
/// <param name="call">The exact model-generated tool call.</param>
/// <param name="exception">The exception raised by the registered tool.</param>
/// <param name="cancellationToken">A token that cancels exception mapping.</param>
/// <returns>
/// Caller-owned output, or null to terminate execution with an exception.
/// </returns>
public delegate ValueTask<MultimodalToolOutput?> MultimodalToolExceptionMapper(
    MultimodalToolCall call,
    Exception exception,
    CancellationToken cancellationToken);
