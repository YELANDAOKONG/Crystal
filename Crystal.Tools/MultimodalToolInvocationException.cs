namespace Crystal.Multimodal.Tools;

/// <summary>Indicates an unmapped failure from a multimodal tool.</summary>
public sealed class MultimodalToolInvocationException : Exception
{
    internal MultimodalToolInvocationException(Exception innerException)
        : base("A registered multimodal tool failed.", innerException)
    {
    }
}
