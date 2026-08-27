namespace Crystal.Completions;

/// <summary>
/// Represents one ordered item in a text-completion candidate.
/// </summary>
public abstract record CompletionItem
{
    private protected CompletionItem()
    {
    }

    /// <inheritdoc />
    public sealed override string ToString() => GetType().Name;
}
