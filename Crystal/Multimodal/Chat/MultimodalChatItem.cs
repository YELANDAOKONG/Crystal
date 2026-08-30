namespace Crystal.Multimodal.Chat;

/// <summary>Represents one ordered item in a multimodal transcript.</summary>
public abstract record MultimodalChatItem
{
    private protected MultimodalChatItem()
    {
    }

    /// <inheritdoc />
    public sealed override string ToString() => GetType().Name;
}
