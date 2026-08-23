namespace Crystal.Tools;

/// <summary>
/// Selects how one tool-call batch is scheduled.
/// </summary>
public enum ToolExecutionMode
{
    /// <summary>
    /// Executes calls one at a time in input order.
    /// </summary>
    Serial,

    /// <summary>
    /// Executes calls concurrently under an explicit concurrency bound.
    /// </summary>
    Concurrent
}
