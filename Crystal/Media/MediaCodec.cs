namespace Crystal.Media;

/// <summary>Contains one media codec identifier.</summary>
public sealed record MediaCodec
{
    /// <summary>Initializes a media codec identifier.</summary>
    /// <param name="value">The complete codec identifier.</param>
    public MediaCodec(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));
        Value = value;
    }

    /// <summary>Gets the complete codec identifier.</summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
