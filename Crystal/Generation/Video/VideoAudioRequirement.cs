namespace Crystal.Generation.Video;

/// <summary>Describes a hard requirement for embedded generated-video audio.</summary>
public sealed record VideoAudioRequirement
{
    /// <summary>Makes no requirement about an embedded audio track.</summary>
    public static VideoAudioRequirement Unspecified { get; } =
        new("unspecified");

    /// <summary>Requires an embedded audio track.</summary>
    public static VideoAudioRequirement Required { get; } = new("required");

    /// <summary>Requires that no embedded audio track be present.</summary>
    public static VideoAudioRequirement Forbidden { get; } = new("forbidden");

    private VideoAudioRequirement(string value)
    {
        Value = value;
    }

    /// <summary>Gets the portable requirement value.</summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
