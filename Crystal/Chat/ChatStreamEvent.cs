namespace Crystal.Chat;

/// <summary>
/// Represents one typed event in a text-chat stream.
/// </summary>
public abstract record ChatStreamEvent
{
    private protected ChatStreamEvent()
    {
    }

    /// <inheritdoc />
    public sealed override string ToString() => GetType().Name;
}
