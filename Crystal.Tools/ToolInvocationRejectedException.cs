namespace Crystal.Tools;

/// <summary>
/// Indicates that a caller-owned policy rejected a tool without returning text.
/// </summary>
public sealed class ToolInvocationRejectedException : Exception
{
    internal ToolInvocationRejectedException()
        : base("A tool invocation was rejected.")
    {
    }
}
