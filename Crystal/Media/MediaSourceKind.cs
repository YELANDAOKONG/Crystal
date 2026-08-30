namespace Crystal.Media;

/// <summary>Identifies one portable media-source lifetime and access shape.</summary>
public sealed record MediaSourceKind
{
    /// <summary>Identifies immutable bytes owned by the source value.</summary>
    public static MediaSourceKind Inline { get; } = new("inline");

    /// <summary>Identifies an absolute URI that Crystal does not fetch.</summary>
    public static MediaSourceKind Uri { get; } = new("uri");

    /// <summary>Identifies a factory that opens a fresh readable stream.</summary>
    public static MediaSourceKind ReplayableStream { get; } =
        new("replayable_stream");

    private MediaSourceKind(string value)
    {
        Value = value;
    }

    /// <summary>Gets the portable source-kind value.</summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
