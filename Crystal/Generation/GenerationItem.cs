namespace Crystal.Generation;

/// <summary>Represents one ordered generation candidate item.</summary>
public abstract record GenerationItem
{
    private protected GenerationItem()
    {
    }

    /// <inheritdoc />
    public sealed override string ToString() => GetType().Name;
}
