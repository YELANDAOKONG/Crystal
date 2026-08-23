namespace Crystal.Tools;

/// <summary>
/// Contains a caller-owned pre-invocation decision.
/// </summary>
public sealed record ToolInvocationDecision
{
    private ToolInvocationDecision(
        ToolInvocationAction action,
        ToolOutput? rejectionOutput)
    {
        Action = action;
        RejectionOutput = rejectionOutput;
    }

    /// <summary>
    /// Gets a decision that allows execution.
    /// </summary>
    public static ToolInvocationDecision Execute { get; } =
        new(ToolInvocationAction.Execute, null);

    /// <summary>
    /// Creates a rejection that terminates execution without model-visible text.
    /// </summary>
    /// <returns>The rejection decision.</returns>
    public static ToolInvocationDecision Reject() =>
        new(ToolInvocationAction.Reject, null);

    /// <summary>
    /// Creates a rejection with exact caller-authored model-visible output.
    /// </summary>
    /// <param name="output">The exact caller-authored rejection output.</param>
    /// <returns>The rejection decision.</returns>
    public static ToolInvocationDecision Reject(ToolOutput output)
    {
        ArgumentNullException.ThrowIfNull(output, nameof(output));
        return new ToolInvocationDecision(ToolInvocationAction.Reject, output);
    }

    /// <summary>
    /// Gets the selected action.
    /// </summary>
    public ToolInvocationAction Action { get; }

    /// <summary>
    /// Gets exact caller-authored rejection output when supplied.
    /// </summary>
    public ToolOutput? RejectionOutput { get; }
}
