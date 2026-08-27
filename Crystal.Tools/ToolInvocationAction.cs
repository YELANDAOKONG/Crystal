namespace Crystal.Tools;

/// <summary>
/// Describes a caller-owned decision made before a tool invocation.
/// </summary>
public enum ToolInvocationAction
{
    /// <summary>
    /// Allows the registered tool to execute.
    /// </summary>
    Execute,

    /// <summary>
    /// Rejects the registered tool invocation.
    /// </summary>
    Reject
}
