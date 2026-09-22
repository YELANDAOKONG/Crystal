namespace Crystal.Multimodal.Chat;

/// <summary>Represents one typed event in a multimodal Chat stream.</summary>
public abstract record MultimodalChatStreamEvent
{
    private protected MultimodalChatStreamEvent()
    {
    }

    /// <inheritdoc />
    public sealed override string ToString() => GetType().Name;
}
