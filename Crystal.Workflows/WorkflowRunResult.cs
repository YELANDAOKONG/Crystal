namespace Crystal.Workflows;

/// <summary>Contains exact terminal outputs and execution accounting.</summary>
/// <typeparam name="TOutput">The graph output type.</typeparam>
public sealed record WorkflowRunResult<TOutput>
    where TOutput : notnull
{
    internal WorkflowRunResult(
        Guid runId,
        IEnumerable<TOutput> outputs,
        WorkflowStopReason stopReason,
        int superstepCount,
        int nodeCallCount)
    {
        RunId = runId;
        Outputs = Array.AsReadOnly(outputs.ToArray());
        StopReason = stopReason;
        SuperstepCount = superstepCount;
        NodeCallCount = nodeCallCount;
    }

    /// <summary>Gets the caller-supplied run identifier.</summary>
    public Guid RunId { get; }

    /// <summary>Gets terminal outputs in execution and message order.</summary>
    public IReadOnlyList<TOutput> Outputs { get; }

    /// <summary>Gets the completion or configured-limit reason.</summary>
    public WorkflowStopReason StopReason { get; }

    /// <summary>Gets the number of executed supersteps.</summary>
    public int SuperstepCount { get; }

    /// <summary>Gets the number of attempted node invocations.</summary>
    public int NodeCallCount { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(WorkflowRunResult<TOutput>);
}
