namespace Crystal.Tools;

/// <summary>
/// Optionally maps a tool exception to exact caller-authored output.
/// </summary>
/// <param name="call">The exact model-generated tool call.</param>
/// <param name="exception">The exception raised by the registered tool.</param>
/// <param name="cancellationToken">
/// A token that cancels exception mapping.
/// </param>
/// <returns>
/// Caller-authored output, or null to terminate execution with an exception.
/// </returns>
public delegate ValueTask<ToolOutput?> ToolExceptionMapper(
    ToolCall call,
    Exception exception,
    CancellationToken cancellationToken);
