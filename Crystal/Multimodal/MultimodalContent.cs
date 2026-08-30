namespace Crystal.Multimodal;

/// <summary>Represents one ordered, strongly typed multimodal content block.</summary>
public abstract record MultimodalContent
{
    private protected MultimodalContent()
    {
    }

    /// <summary>Gets the block modality.</summary>
    public abstract ContentModality Modality { get; }

    /// <inheritdoc />
    public sealed override string ToString() => GetType().Name;
}
