namespace Crystal.Decisions.Results;

/// <summary>Reports the probability that a condition is true.</summary>
public sealed record PredicateResult : DecisionResult
{
    /// <summary>Initializes a predicate result.</summary>
    public PredicateResult(string questionId, double probability, double? confidence = null)
        : base(questionId)
    {
        DecisionContract.ValidateProbability(probability, nameof(probability));
        if (confidence is double reportedConfidence)
        {
            DecisionContract.ValidateProbability(reportedConfidence, nameof(confidence));
        }

        Probability = probability;
        Confidence = confidence;
    }

    /// <summary>Gets the model-reported probability of the true outcome.</summary>
    public double Probability { get; }

    /// <summary>Gets confidence when separately reported by the service.</summary>
    public double? Confidence { get; }
}
