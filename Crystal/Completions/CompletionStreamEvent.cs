namespace Crystal.Completions;

/// <summary>
/// Represents one typed event in a completion stream.
/// </summary>
public abstract record CompletionStreamEvent
{
    private protected CompletionStreamEvent()
    {
    }

    /// <inheritdoc />
    public sealed override string ToString() => GetType().Name;
}
