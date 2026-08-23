namespace Crystal.Tools;

/// <summary>
/// Indicates that a model requested an unregistered tool.
/// </summary>
public sealed class ToolNotFoundException : Exception
{
    internal ToolNotFoundException()
        : base("The requested tool is not registered.")
    {
    }
}
