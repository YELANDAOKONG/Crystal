using Crystal.Chat;

namespace Crystal.Tools;

/// <summary>
/// Contains one exact textual tool result correlated to a model call.
/// </summary>
public sealed record ToolResult : ChatItem
{
    /// <summary>
    /// Initializes a successful tool result.
    /// </summary>
    /// <param name="callId">The model-generated correlation identifier.</param>
    /// <param name="text">The exact caller-owned result text.</param>
    public ToolResult(
        string callId,
        string text)
        : this(callId, text, ToolResultStatus.Success)
    {
    }

    /// <summary>
    /// Initializes a tool result.
    /// </summary>
    /// <param name="callId">The model-generated correlation identifier.</param>
    /// <param name="text">The exact caller-owned result text.</param>
    /// <param name="status">The caller-declared result status.</param>
    public ToolResult(
        string callId,
        string text,
        ToolResultStatus status)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(callId, nameof(callId));
        ArgumentNullException.ThrowIfNull(text, nameof(text));
        ArgumentNullException.ThrowIfNull(status, nameof(status));

        CallId = callId;
        Text = text;
        Status = status;
    }

    /// <summary>
    /// Gets the model-generated correlation identifier.
    /// </summary>
    public string CallId { get; }

    /// <summary>
    /// Gets the exact caller-owned result text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the caller-declared result status.
    /// </summary>
    public ToolResultStatus Status { get; }
}
