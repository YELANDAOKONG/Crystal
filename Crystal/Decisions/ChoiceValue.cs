namespace Crystal.Decisions;

/// <summary>Preserves the exact value and type of a closed-choice option.</summary>
public sealed record ChoiceValue
{
    /// <summary>Initializes a string option value.</summary>
    public ChoiceValue(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Text = value;
    }

    /// <summary>Initializes a boolean option value.</summary>
    public ChoiceValue(bool value)
    {
        Boolean = value;
    }

    /// <summary>Gets the exact value for a string option.</summary>
    public string? Text { get; }

    /// <summary>Gets the exact value for a boolean option.</summary>
    public bool? Boolean { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(ChoiceValue);
}
