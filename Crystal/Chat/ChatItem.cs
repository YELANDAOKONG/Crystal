namespace Crystal.Chat;

/// <summary>
/// Represents one ordered item in a text-chat transcript.
/// </summary>
public abstract record ChatItem
{
    private protected ChatItem()
    {
    }

    /// <inheritdoc />
    public sealed override string ToString() => GetType().Name;
}
