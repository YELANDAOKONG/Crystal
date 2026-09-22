namespace Crystal.Multimodal.Chat;

/// <summary>Starts one ordered multimodal message item.</summary>
public sealed record MultimodalMessageStarted : MultimodalChatItemStreamEvent
{
    /// <summary>Initializes a message-started event.</summary>
    /// <param name="candidateIndex">The zero-based candidate index.</param>
    /// <param name="itemIndex">The zero-based item index.</param>
    /// <param name="role">The role of the message being streamed.</param>
    public MultimodalMessageStarted(
        int candidateIndex,
        int itemIndex,
        MultimodalChatRole role)
        : base(candidateIndex, itemIndex)
    {
        ArgumentNullException.ThrowIfNull(role, nameof(role));
        Role = role;
    }

    /// <summary>Gets the role of the message being streamed.</summary>
    public MultimodalChatRole Role { get; }
}
