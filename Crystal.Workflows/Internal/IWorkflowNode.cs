namespace Crystal.Workflows.Internal;

internal interface IWorkflowNode
{
    string Name { get; }

    Task<IReadOnlyList<object>> InvokeAsync(
        IReadOnlyList<object> inputs,
        CancellationToken cancellationToken);
}
