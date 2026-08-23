namespace Crystal.Tools;

/// <summary>
/// Indicates that a registered tool failed without a caller-owned text mapping.
/// </summary>
public sealed class ToolInvocationException : Exception
{
    internal ToolInvocationException(Exception innerException)
        : base("A registered tool failed.", innerException)
    {
    }
}
