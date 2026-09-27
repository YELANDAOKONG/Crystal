using Crystal.Internal;
using Crystal.Multimodal.Chat;
using Crystal.Reasoning;
using Crystal.Tools;

namespace Crystal.Realtime;

/// <summary>Contains caller-owned requirements for one live media session.</summary>
public sealed record RealtimeSessionRequest
{
    /// <summary>Initializes a live session request.</summary>
    /// <param name="outputModalities">Non-empty required output modalities.</param>
    /// <param name="turnMode">Who determines input turn boundaries.</param>
    /// <param name="initialContext">Exact caller-retained ordered context.</param>
    /// <param name="tools">Optional caller-authored tool definitions.</param>
    /// <param name="reasoning">Optional portable reasoning hints.</param>
    public RealtimeSessionRequest(
        IEnumerable<ContentModality> outputModalities,
        RealtimeTurnMode turnMode,
        IEnumerable<MultimodalChatItem>? initialContext = null,
        IEnumerable<ToolDefinition>? tools = null,
        ReasoningOptions? reasoning = null)
    {
        if (!Enum.IsDefined(turnMode))
        {
            throw new ArgumentOutOfRangeException(nameof(turnMode));
        }

        OutputModalities = CollectionSnapshot.Create(
            outputModalities, nameof(outputModalities), allowEmpty: false);
        if (OutputModalities.Distinct().Count() != OutputModalities.Count)
        {
            throw new ArgumentException("Output modalities must be unique.", nameof(outputModalities));
        }

        InitialContext = CollectionSnapshot.Create(
            initialContext ?? Array.Empty<MultimodalChatItem>(), nameof(initialContext));
        Tools = CollectionSnapshot.Create(
            tools ?? Array.Empty<ToolDefinition>(), nameof(tools));
        if (Tools.Select(static tool => tool.Name)
            .Distinct(StringComparer.Ordinal)
            .Count() != Tools.Count)
        {
            throw new ArgumentException("Tool names must be unique.", nameof(tools));
        }

        TurnMode = turnMode;
        Reasoning = reasoning;
    }

    /// <summary>Gets exact required output modalities in caller order.</summary>
    public IReadOnlyList<ContentModality> OutputModalities { get; }

    /// <summary>Gets who determines input turn boundaries.</summary>
    public RealtimeTurnMode TurnMode { get; }

    /// <summary>Gets exact caller-retained context in order.</summary>
    public IReadOnlyList<MultimodalChatItem> InitialContext { get; }

    /// <summary>Gets caller-authored tools in order.</summary>
    public IReadOnlyList<ToolDefinition> Tools { get; }

    /// <summary>Gets optional portable reasoning hints.</summary>
    public ReasoningOptions? Reasoning { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(RealtimeSessionRequest);
}
