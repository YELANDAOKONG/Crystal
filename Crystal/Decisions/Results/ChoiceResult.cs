using Crystal.Internal;

namespace Crystal.Decisions.Results;

/// <summary>Reports the selected value and per-option probabilities.</summary>
public sealed record ChoiceResult : DecisionResult
{
    /// <summary>Initializes a choice result with probabilities in request option order.</summary>
    public ChoiceResult(
        string questionId,
        ChoiceValue selectedValue,
        IEnumerable<ChoiceProbability> probabilities,
        double? confidence = null)
        : base(questionId)
    {
        ArgumentNullException.ThrowIfNull(selectedValue);
        Probabilities = CollectionSnapshot.Create(probabilities, nameof(probabilities), allowEmpty: false);
        var values = Probabilities.Select(static entry => entry.Value).ToHashSet();
        if (values.Count != Probabilities.Count || !values.Contains(selectedValue))
        {
            throw new ArgumentException(
                "Choice probabilities must identify unique values and include the selected value.",
                nameof(probabilities));
        }

        if (confidence is double reportedConfidence)
        {
            DecisionContract.ValidateProbability(reportedConfidence, nameof(confidence));
        }

        SelectedValue = selectedValue;
        Confidence = confidence;
    }

    /// <summary>Gets the option selected by the service.</summary>
    public ChoiceValue SelectedValue { get; }

    /// <summary>Gets the reported probability for each option in request order.</summary>
    public IReadOnlyList<ChoiceProbability> Probabilities { get; }

    /// <summary>Gets confidence when separately reported by the service.</summary>
    public double? Confidence { get; }
}
