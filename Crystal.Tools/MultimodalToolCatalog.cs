using Crystal.Internal;
using Crystal.Tools;

namespace Crystal.Multimodal.Tools;

/// <summary>Provides immutable lookup for caller-owned multimodal tools.</summary>
public sealed class MultimodalToolCatalog
{
    private readonly IReadOnlyDictionary<string, IMultimodalTool> _tools;

    /// <summary>Initializes a multimodal tool catalog.</summary>
    /// <param name="tools">The multimodal tools to register.</param>
    public MultimodalToolCatalog(IEnumerable<IMultimodalTool> tools)
    {
        var snapshot = CollectionSnapshot.Create(tools, nameof(tools));
        var registrations = new Dictionary<string, IMultimodalTool>(
            snapshot.Count,
            StringComparer.Ordinal);
        var definitions = new ToolDefinition[snapshot.Count];

        for (var index = 0; index < snapshot.Count; index++)
        {
            var tool = snapshot[index];
            var definition = tool.Definition
                ?? throw new ArgumentException(
                    "A multimodal tool returned no definition.",
                    nameof(tools));

            if (!registrations.TryAdd(definition.Name, tool))
            {
                throw new ArgumentException(
                    "Multimodal tool names must be unique.",
                    nameof(tools));
            }

            definitions[index] = definition;
        }

        _tools = registrations;
        Definitions = Array.AsReadOnly(definitions);
    }

    /// <summary>Gets definitions in registration order.</summary>
    public IReadOnlyList<ToolDefinition> Definitions { get; }

    /// <summary>Finds a multimodal tool by its case-sensitive name.</summary>
    /// <param name="name">The tool name.</param>
    /// <returns>The registered tool, or null when no tool matches.</returns>
    public IMultimodalTool? Find(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        return _tools.GetValueOrDefault(name);
    }
}
