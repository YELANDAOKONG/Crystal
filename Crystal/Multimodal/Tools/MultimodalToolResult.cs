using Crystal.Internal;
using Crystal.Multimodal.Chat;

namespace Crystal.Multimodal.Tools;

/// <summary>Contains one exact multimodal result correlated to a tool call.</summary>
public sealed record MultimodalToolResult : MultimodalChatItem
{
    /// <summary>Initializes a successful multimodal tool result.</summary>
    /// <param name="callId">The model-generated correlation identifier.</param>
    /// <param name="contents">The ordered caller-owned content.</param>
    public MultimodalToolResult(
        string callId,
        IEnumerable<MultimodalContent> contents)
        : this(callId, contents, MultimodalToolResultStatus.Success)
    {
    }

    /// <summary>Initializes a multimodal tool result.</summary>
    /// <param name="callId">The model-generated correlation identifier.</param>
    /// <param name="contents">The ordered caller-owned content.</param>
    /// <param name="status">The caller-declared result status.</param>
    public MultimodalToolResult(
        string callId,
        IEnumerable<MultimodalContent> contents,
        MultimodalToolResultStatus status)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(callId, nameof(callId));
        ArgumentNullException.ThrowIfNull(status, nameof(status));

        CallId = callId;
        Contents = CollectionSnapshot.Create(contents, nameof(contents));
        Status = status;
    }

    /// <summary>Gets the model-generated correlation identifier.</summary>
    public string CallId { get; }

    /// <summary>Gets the ordered caller-owned result content.</summary>
    public IReadOnlyList<MultimodalContent> Contents { get; }

    /// <summary>Gets the caller-declared result status.</summary>
    public MultimodalToolResultStatus Status { get; }
}
