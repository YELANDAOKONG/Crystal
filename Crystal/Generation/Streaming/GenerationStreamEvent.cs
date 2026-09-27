namespace Crystal.Generation.Streaming;

/// <summary>Represents one typed target-specific generation stream event.</summary>
/// <typeparam name="TResponse">The target-specific complete response type.</typeparam>
public abstract record GenerationStreamEvent<TResponse>
    where TResponse : class
{
    private protected GenerationStreamEvent()
    {
    }

    /// <inheritdoc />
    public sealed override string ToString() => GetType().Name;
}
