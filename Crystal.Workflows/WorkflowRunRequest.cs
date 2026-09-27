namespace Crystal.Workflows;

/// <summary>Contains one caller-authored workflow invocation.</summary>
/// <typeparam name="TInput">The graph input type.</typeparam>
public sealed record WorkflowRunRequest<TInput>
    where TInput : notnull
{
    /// <summary>Initializes a workflow invocation.</summary>
    /// <param name="runId">The caller-supplied non-empty identifier.</param>
    /// <param name="inputs">The ordered inputs, which may be empty.</param>
    /// <param name="limits">Optional execution limits.</param>
    public WorkflowRunRequest(
        Guid runId,
        IEnumerable<TInput> inputs,
        WorkflowLimits? limits = null)
    {
        if (runId == Guid.Empty)
        {
            throw new ArgumentException(
                "Run identifier cannot be empty.",
                nameof(runId));
        }

        ArgumentNullException.ThrowIfNull(inputs, nameof(inputs));
        var snapshot = inputs.ToArray();
        if (snapshot.Any(static input => input is null))
        {
            throw new ArgumentException(
                "Workflow inputs cannot contain null values.",
                nameof(inputs));
        }

        RunId = runId;
        Inputs = Array.AsReadOnly(snapshot);
        Limits = limits ?? WorkflowLimits.Unlimited;
    }

    /// <summary>Gets the caller-supplied run identifier.</summary>
    public Guid RunId { get; }

    /// <summary>Gets the exact ordered input snapshot.</summary>
    public IReadOnlyList<TInput> Inputs { get; }

    /// <summary>Gets the execution bounds.</summary>
    public WorkflowLimits Limits { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(WorkflowRunRequest<TInput>);
}
