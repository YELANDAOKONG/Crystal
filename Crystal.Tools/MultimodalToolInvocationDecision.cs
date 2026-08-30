namespace Crystal.Multimodal.Tools;

/// <summary>Contains a caller-owned pre-invocation decision.</summary>
public sealed record MultimodalToolInvocationDecision
{
    private MultimodalToolInvocationDecision(
        MultimodalToolInvocationAction action,
        MultimodalToolOutput? rejectionOutput)
    {
        Action = action;
        RejectionOutput = rejectionOutput;
    }

    /// <summary>Gets a decision that allows execution.</summary>
    public static MultimodalToolInvocationDecision Execute { get; } =
        new(MultimodalToolInvocationAction.Execute, null);

    /// <summary>Creates a rejection without model-visible content.</summary>
    /// <returns>The rejection decision.</returns>
    public static MultimodalToolInvocationDecision Reject() =>
        new(MultimodalToolInvocationAction.Reject, null);

    /// <summary>Creates a rejection with exact caller-owned output.</summary>
    /// <param name="output">The exact caller-owned rejection output.</param>
    /// <returns>The rejection decision.</returns>
    public static MultimodalToolInvocationDecision Reject(
        MultimodalToolOutput output)
    {
        ArgumentNullException.ThrowIfNull(output, nameof(output));
        return new MultimodalToolInvocationDecision(
            MultimodalToolInvocationAction.Reject,
            output);
    }

    /// <summary>Gets the selected action.</summary>
    public MultimodalToolInvocationAction Action { get; }

    /// <summary>Gets caller-owned rejection output when supplied.</summary>
    public MultimodalToolOutput? RejectionOutput { get; }
}
