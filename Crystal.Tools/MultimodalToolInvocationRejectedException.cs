namespace Crystal.Multimodal.Tools;

/// <summary>Indicates a policy rejection without caller-owned output.</summary>
public sealed class MultimodalToolInvocationRejectedException : Exception
{
    internal MultimodalToolInvocationRejectedException()
        : base("A multimodal tool invocation was rejected.")
    {
    }
}
