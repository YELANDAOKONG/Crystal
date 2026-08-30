namespace Crystal.Media;

/// <summary>Contains one explicit media MIME type.</summary>
public sealed record MediaMimeType
{
    /// <summary>Initializes a media MIME type.</summary>
    /// <param name="value">The complete MIME type value.</param>
    public MediaMimeType(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));
        Value = value;
    }

    /// <summary>Gets the complete MIME type value.</summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
