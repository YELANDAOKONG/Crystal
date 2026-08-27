using Crystal.Internal;

namespace Crystal.Tools;

/// <summary>
/// Provides immutable case-sensitive lookup for caller-owned tools.
/// </summary>
public sealed class ToolCatalog
{
    private readonly IReadOnlyDictionary<string, ITool> _tools;

    /// <summary>
    /// Initializes a tool catalog.
    /// </summary>
    /// <param name="tools">The tools to register.</param>
    public ToolCatalog(IEnumerable<ITool> tools)
    {
        var snapshot = CollectionSnapshot.Create(tools, nameof(tools));
        var registrations = new Dictionary<string, ITool>(
            snapshot.Count,
            StringComparer.Ordinal);
        var definitions = new ToolDefinition[snapshot.Count];

        for (var index = 0; index < snapshot.Count; index++)
        {
            var tool = snapshot[index];
            var definition = tool.Definition
                ?? throw new ArgumentException(
                    "A tool returned no definition.",
                    nameof(tools));

            if (!registrations.TryAdd(definition.Name, tool))
            {
                throw new ArgumentException(
                    "Tool names must be unique.",
                    nameof(tools));
            }

            definitions[index] = definition;
        }

        _tools = registrations;
        Definitions = Array.AsReadOnly(definitions);
    }

    /// <summary>
    /// Gets definitions in registration order.
    /// </summary>
    public IReadOnlyList<ToolDefinition> Definitions { get; }

    /// <summary>
    /// Finds a tool by its case-sensitive name.
    /// </summary>
    /// <param name="name">The tool name.</param>
    /// <returns>The registered tool, or null when no tool matches.</returns>
    public ITool? Find(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        return _tools.GetValueOrDefault(name);
    }
}
