namespace Crystal.Decisions.Results;

/// <summary>Reports the probability of one exact choice value.</summary>
public sealed record ChoiceProbability
{
    /// <summary>Initializes the probability reported for a choice.</summary>
    public ChoiceProbability(ChoiceValue value, double probability)
    {
        ArgumentNullException.ThrowIfNull(value);
        DecisionContract.ValidateProbability(probability, nameof(probability));
        Value = value;
        Probability = probability;
    }

    /// <summary>Gets the original option value.</summary>
    public ChoiceValue Value { get; }

    /// <summary>Gets the probability reported for that option.</summary>
    public double Probability { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(ChoiceProbability);
}
