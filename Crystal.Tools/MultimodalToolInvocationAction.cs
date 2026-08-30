namespace Crystal.Multimodal.Tools;

/// <summary>Describes a decision made before multimodal tool invocation.</summary>
public enum MultimodalToolInvocationAction
{
    /// <summary>Allows the registered multimodal tool to execute.</summary>
    Execute,

    /// <summary>Rejects the registered multimodal tool invocation.</summary>
    Reject
}
