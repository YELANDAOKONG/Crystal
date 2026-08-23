namespace Crystal.Chat;

/// <summary>
/// Carries one exact text delta for a chat message.
/// </summary>
public sealed record ChatTextDelta : ChatItemStreamEvent
{
    /// <summary>
    /// Initializes a chat text delta.
    /// </summary>
    /// <param name="candidateIndex">The zero-based candidate index.</param>
    /// <param name="itemIndex">The zero-based item index.</param>
    /// <param name="role">The role of the message being streamed.</param>
    /// <param name="text">The exact text delta, which may be empty.</param>
    public ChatTextDelta(
        int candidateIndex,
        int itemIndex,
        ChatRole role,
        string text)
        : base(candidateIndex, itemIndex)
    {
        ArgumentNullException.ThrowIfNull(role, nameof(role));
        ArgumentNullException.ThrowIfNull(text, nameof(text));

        Role = role;
        Text = text;
    }

    /// <summary>
    /// Gets the role of the message being streamed.
    /// </summary>
    public ChatRole Role { get; }

    /// <summary>
    /// Gets the exact text delta.
    /// </summary>
    public string Text { get; }
}
