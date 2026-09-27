namespace Crystal.Workflows.Internal;

internal interface IWorkflowEdge
{
    IWorkflowNode Source { get; }

    IWorkflowNode Target { get; }

    ValueTask<bool> AcceptsAsync(
        object message,
        CancellationToken cancellationToken);
}
