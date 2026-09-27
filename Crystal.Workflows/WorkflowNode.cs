using Crystal.Workflows.Internal;

namespace Crystal.Workflows;

/// <summary>Executes one caller-owned typed operation in a workflow graph.</summary>
/// <typeparam name="TInput">The exact input message type.</typeparam>
/// <typeparam name="TOutput">The exact output message type.</typeparam>
public sealed class WorkflowNode<TInput, TOutput> : IWorkflowNode
    where TInput : notnull
    where TOutput : notnull
{
    private readonly Func<IReadOnlyList<TInput>, CancellationToken,
        Task<IReadOnlyList<TOutput>>> _operation;

    /// <summary>Initializes a named node with a caller-owned operation.</summary>
    /// <param name="name">A stable unique name within the graph.</param>
    /// <param name="operation">An operation over the ordered input batch.</param>
    public WorkflowNode(
        string name,
        Func<IReadOnlyList<TInput>, CancellationToken,
            Task<IReadOnlyList<TOutput>>> operation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentNullException.ThrowIfNull(operation, nameof(operation));

        Name = name;
        _operation = operation;
    }

    /// <summary>Gets the stable node name.</summary>
    public string Name { get; }

    async Task<IReadOnlyList<object>> IWorkflowNode.InvokeAsync(
        IReadOnlyList<object> inputs,
        CancellationToken cancellationToken)
    {
        var typedInputs = new TInput[inputs.Count];
        for (var index = 0; index < inputs.Count; index++)
        {
            typedInputs[index] = (TInput)inputs[index];
        }

        var operation = _operation(
            Array.AsReadOnly(typedInputs),
            cancellationToken)
            ?? throw new InvalidOperationException(
                "The workflow node returned no operation.");
        var outputs = await operation.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        if (outputs is null)
        {
            throw new InvalidOperationException(
                "The workflow node returned no output collection.");
        }

        var snapshot = new object[outputs.Count];
        for (var index = 0; index < outputs.Count; index++)
        {
            snapshot[index] = outputs[index]
                ?? throw new InvalidOperationException(
                    "The workflow node returned a null output.");
        }

        return Array.AsReadOnly(snapshot);
    }
}
