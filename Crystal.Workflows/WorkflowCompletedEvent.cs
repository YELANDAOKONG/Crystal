namespace Crystal.Workflows;

/// <summary>Contains the exact final workflow result.</summary>
/// <typeparam name="TOutput">The graph output type.</typeparam>
public sealed class WorkflowCompletedEvent<TOutput> : WorkflowEvent
    where TOutput : notnull
{
    internal WorkflowCompletedEvent(
        Guid runId,
        long sequence,
        int superstep,
        WorkflowRunResult<TOutput> result)
        : base(runId, sequence, superstep)
    {
        Result = result;
    }

    /// <summary>Gets the exact terminal result.</summary>
    public WorkflowRunResult<TOutput> Result { get; }
}
