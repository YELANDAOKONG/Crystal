namespace Crystal.Generation;

/// <summary>Represents one ordered, strongly typed generation input.</summary>
public abstract record GenerationInput
{
    private protected GenerationInput()
    {
    }

    /// <summary>Gets the input modality.</summary>
    public abstract ContentModality Modality { get; }

    /// <summary>Gets the portable input purpose.</summary>
    public abstract GenerationInputPurpose Purpose { get; }

    /// <inheritdoc />
    public sealed override string ToString() => GetType().Name;
}
