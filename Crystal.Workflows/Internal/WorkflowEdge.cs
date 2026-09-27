namespace Crystal.Workflows.Internal;

internal sealed class WorkflowEdge<TMessage> : IWorkflowEdge
    where TMessage : notnull
{
    private readonly Func<TMessage, CancellationToken, ValueTask<bool>>?
        _condition;

    public WorkflowEdge(
        IWorkflowNode source,
        IWorkflowNode target,
        Func<TMessage, CancellationToken, ValueTask<bool>>? condition)
    {
        Source = source;
        Target = target;
        _condition = condition;
    }

    public IWorkflowNode Source { get; }

    public IWorkflowNode Target { get; }

    public Delegate? Condition => _condition;

    public ValueTask<bool> AcceptsAsync(
        object message,
        CancellationToken cancellationToken) =>
        _condition is null
            ? ValueTask.FromResult(true)
            : _condition((TMessage)message, cancellationToken);
}
