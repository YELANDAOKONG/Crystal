namespace Crystal.Generation;

/// <summary>Identifies one portable purpose for a generation input.</summary>
public sealed record GenerationInputPurpose
{
    /// <summary>Identifies caller-authored text instructions.</summary>
    public static GenerationInputPurpose Instruction { get; } =
        new("instruction");

    /// <summary>Identifies conditioning reference media.</summary>
    public static GenerationInputPurpose Reference { get; } =
        new("reference");

    /// <summary>Identifies media to transform or continue.</summary>
    public static GenerationInputPurpose Source { get; } = new("source");

    /// <summary>Identifies an image mask.</summary>
    public static GenerationInputPurpose Mask { get; } = new("mask");

    /// <summary>Identifies the requested first video frame.</summary>
    public static GenerationInputPurpose FirstFrame { get; } =
        new("first_frame");

    /// <summary>Identifies the requested last video frame.</summary>
    public static GenerationInputPurpose LastFrame { get; } =
        new("last_frame");

    private GenerationInputPurpose(string value)
    {
        Value = value;
    }

    /// <summary>Gets the portable purpose value.</summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
