using Crystal.Chat;

namespace Crystal.Tools;

/// <summary>
/// Contains one complete model-generated tool call.
/// </summary>
public sealed record ToolCall : ChatItem
{
    /// <summary>
    /// Initializes a tool call.
    /// </summary>
    /// <param name="callId">The model-generated correlation identifier.</param>
    /// <param name="name">The requested case-sensitive tool name.</param>
    /// <param name="arguments">
    /// The exact raw model-generated argument text.
    /// </param>
    public ToolCall(
        string callId,
        string name,
        string arguments)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(callId, nameof(callId));
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentNullException.ThrowIfNull(arguments, nameof(arguments));

        CallId = callId;
        Name = name;
        Arguments = arguments;
    }

    /// <summary>
    /// Gets the correlation identifier.
    /// </summary>
    public string CallId { get; }

    /// <summary>
    /// Gets the requested tool name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the exact raw argument text.
    /// </summary>
    public string Arguments { get; }
}
