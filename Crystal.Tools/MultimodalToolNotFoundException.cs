namespace Crystal.Multimodal.Tools;

/// <summary>Indicates that a model requested an unregistered tool.</summary>
public sealed class MultimodalToolNotFoundException : Exception
{
    internal MultimodalToolNotFoundException()
        : base("The requested multimodal tool is not registered.")
    {
    }
}
