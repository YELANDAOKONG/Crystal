namespace Crystal.Decisions;

/// <summary>Declares one supported non-media content shape at one request position.</summary>
public sealed record DecisionDataCapability
{
    /// <summary>Initializes a supported decision data shape.</summary>
    public DecisionDataCapability(DecisionContentPosition position, DecisionDataKind kind)
    {
        if (!Enum.IsDefined(position))
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind));
        }

        Position = position;
        Kind = kind;
    }

    /// <summary>Gets the supported request position.</summary>
    public DecisionContentPosition Position { get; }

    /// <summary>Gets the supported data shape.</summary>
    public DecisionDataKind Kind { get; }
}
