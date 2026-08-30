namespace Crystal.Media;

/// <summary>
/// Describes whether a video representation contains an embedded audio track.
/// </summary>
public sealed record VideoAudioPresence
{
    /// <summary>Indicates that embedded-audio presence is unknown.</summary>
    public static VideoAudioPresence Unknown { get; } = new("unknown");

    /// <summary>Indicates that no embedded audio track is present.</summary>
    public static VideoAudioPresence Absent { get; } = new("absent");

    /// <summary>Indicates that an embedded audio track is present.</summary>
    public static VideoAudioPresence Present { get; } = new("present");

    private VideoAudioPresence(string value)
    {
        Value = value;
    }

    /// <summary>Gets the portable presence value.</summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
