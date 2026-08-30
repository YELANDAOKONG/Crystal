namespace Crystal;

/// <summary>
/// Identifies one portable model-content modality.
/// </summary>
public sealed record ContentModality
{
    /// <summary>Identifies exact text content.</summary>
    public static ContentModality Text { get; } = new("text");

    /// <summary>Identifies image content.</summary>
    public static ContentModality Image { get; } = new("image");

    /// <summary>Identifies audio content.</summary>
    public static ContentModality Audio { get; } = new("audio");

    /// <summary>Identifies video content.</summary>
    public static ContentModality Video { get; } = new("video");

    private ContentModality(string value)
    {
        Value = value;
    }

    /// <summary>Gets the portable modality value.</summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
