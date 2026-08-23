namespace Crystal.Chat;

/// <summary>
/// Contains one exact text message with an explicit role.
/// </summary>
public sealed record ChatMessage : ChatItem
{
    /// <summary>
    /// Initializes a text-chat message.
    /// </summary>
    /// <param name="role">The message role.</param>
    /// <param name="text">The exact message text.</param>
    public ChatMessage(ChatRole role, string text)
    {
        ArgumentNullException.ThrowIfNull(role, nameof(role));
        ArgumentNullException.ThrowIfNull(text, nameof(text));

        Role = role;
        Text = text;
    }

    /// <summary>
    /// Gets the message role.
    /// </summary>
    public ChatRole Role { get; }

    /// <summary>
    /// Gets the exact message text.
    /// </summary>
    public string Text { get; }
}
