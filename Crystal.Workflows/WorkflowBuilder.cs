using Crystal.Workflows.Internal;

namespace Crystal.Workflows;

/// <summary>Builds a typed caller-owned workflow graph.</summary>
/// <typeparam name="TInput">The graph input type.</typeparam>
/// <typeparam name="TStartOutput">The starting node output type.</typeparam>
public sealed class WorkflowBuilder<TInput, TStartOutput>
    where TInput : notnull
    where TStartOutput : notnull
{
    private readonly WorkflowNode<TInput, TStartOutput> _start;
    private readonly List<IWorkflowNode> _nodes = [];
    private readonly List<IWorkflowEdge> _edges = [];

    /// <summary>Initializes a graph with one starting node.</summary>
    /// <param name="start">The first node to invoke for each run.</param>
    public WorkflowBuilder(WorkflowNode<TInput, TStartOutput> start)
    {
        ArgumentNullException.ThrowIfNull(start, nameof(start));
        _start = start;
        _nodes.Add(start);
    }

    /// <summary>Adds a type-matched edge with an optional caller-owned condition.</summary>
    /// <typeparam name="TSourceInput">The source node input type.</typeparam>
    /// <typeparam name="TMessage">The routed message type.</typeparam>
    /// <typeparam name="TTargetOutput">The target node output type.</typeparam>
    /// <param name="source">The source node.</param>
    /// <param name="target">The target node.</param>
    /// <param name="condition">An optional per-message routing condition.</param>
    /// <returns>This builder for further configuration.</returns>
    public WorkflowBuilder<TInput, TStartOutput> AddEdge<
        TSourceInput, TMessage, TTargetOutput>(
        WorkflowNode<TSourceInput, TMessage> source,
        WorkflowNode<TMessage, TTargetOutput> target,
        Func<TMessage, CancellationToken, ValueTask<bool>>? condition = null)
        where TSourceInput : notnull
        where TMessage : notnull
        where TTargetOutput : notnull
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(target, nameof(target));

        Register(source);
        Register(target);
        _edges.Add(new WorkflowEdge<TMessage>(source, target, condition));
        return this;
    }

    /// <summary>Validates and snapshots the graph with one terminal node.</summary>
    /// <typeparam name="TFinalInput">The terminal node input type.</typeparam>
    /// <typeparam name="TOutput">The graph output type.</typeparam>
    /// <param name="terminal">The sole terminal node.</param>
    /// <returns>An immutable executable workflow.</returns>
    public Workflow<TInput, TOutput> Build<TFinalInput, TOutput>(
        WorkflowNode<TFinalInput, TOutput> terminal)
        where TFinalInput : notnull
        where TOutput : notnull
    {
        ArgumentNullException.ThrowIfNull(terminal, nameof(terminal));

        if (!_nodes.Contains(terminal))
        {
            throw new ArgumentException(
                "The terminal node is not registered in the graph.",
                nameof(terminal));
        }

        if (_nodes.Select(static node => node.Name)
            .Distinct(StringComparer.Ordinal).Count() != _nodes.Count)
        {
            throw new InvalidOperationException(
                "Workflow node names must be unique.");
        }

        if (_edges.Any(edge => edge.Source == terminal))
        {
            throw new InvalidOperationException(
                "The terminal node cannot have outgoing edges.");
        }

        if (ReachableFrom(_start, reverse: false).Count != _nodes.Count
            || ReachableFrom(terminal, reverse: true).Count != _nodes.Count)
        {
            throw new InvalidOperationException(
                "Every workflow node must lie on a path from start to terminal.");
        }

        return new Workflow<TInput, TOutput>(
            _start,
            terminal,
            _nodes.ToArray(),
            _edges.ToArray());
    }

    private void Register(IWorkflowNode node)
    {
        if (!_nodes.Contains(node))
        {
            _nodes.Add(node);
        }
    }

    private HashSet<IWorkflowNode> ReachableFrom(
        IWorkflowNode start,
        bool reverse)
    {
        var reached = new HashSet<IWorkflowNode> { start };
        var pending = new Queue<IWorkflowNode>();
        pending.Enqueue(start);

        while (pending.Count > 0)
        {
            var node = pending.Dequeue();
            foreach (var edge in _edges)
            {
                var adjacent = reverse ? edge.Source : edge.Target;
                if ((reverse ? edge.Target : edge.Source) == node
                    && reached.Add(adjacent))
                {
                    pending.Enqueue(adjacent);
                }
            }
        }

        return reached;
    }
}
