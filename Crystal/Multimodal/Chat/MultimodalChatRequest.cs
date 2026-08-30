using Crystal.Internal;
using Crystal.Reasoning;
using Crystal.Tools;

namespace Crystal.Multimodal.Chat;

/// <summary>Contains one provider-neutral multimodal Chat request.</summary>
public sealed record MultimodalChatRequest
{
    /// <summary>Initializes a multimodal Chat request.</summary>
    /// <param name="items">The exact ordered multimodal transcript.</param>
    /// <param name="tools">The caller-authored tool definitions.</param>
    /// <param name="reasoning">Optional portable reasoning hints.</param>
    public MultimodalChatRequest(
        IEnumerable<MultimodalChatItem> items,
        IEnumerable<ToolDefinition>? tools = null,
        ReasoningOptions? reasoning = null)
    {
        Items = CollectionSnapshot.Create(items, nameof(items));
        Tools = CollectionSnapshot.Create(
            tools ?? Array.Empty<ToolDefinition>(),
            nameof(tools));

        if (Tools.Select(static tool => tool.Name)
            .Distinct(StringComparer.Ordinal)
            .Count() != Tools.Count)
        {
            throw new ArgumentException(
                "Tool definitions must have unique names.",
                nameof(tools));
        }

        Reasoning = reasoning;
    }

    /// <summary>Gets the exact ordered multimodal transcript.</summary>
    public IReadOnlyList<MultimodalChatItem> Items { get; }

    /// <summary>Gets the caller-authored tool definitions.</summary>
    public IReadOnlyList<ToolDefinition> Tools { get; }

    /// <summary>Gets optional portable reasoning hints.</summary>
    public ReasoningOptions? Reasoning { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(MultimodalChatRequest);
}
