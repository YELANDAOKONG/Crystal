using Crystal.Internal;

namespace Crystal.Decisions.Results;

/// <summary>Reports the supplied rubric's expected score and per-level probabilities.</summary>
public sealed record ScoreResult : DecisionResult
{
    /// <summary>Initializes a score result with probabilities in request level order.</summary>
    public ScoreResult(
        string questionId,
        double score,
        IEnumerable<double> probabilities,
        double? confidence = null)
        : base(questionId)
    {
        if (!double.IsFinite(score))
        {
            throw new ArgumentOutOfRangeException(nameof(score), "Score must be finite.");
        }

        Probabilities = CollectionSnapshot.Create(probabilities, nameof(probabilities), allowEmpty: false);
        foreach (var probability in Probabilities)
        {
            DecisionContract.ValidateProbability(probability, nameof(probabilities));
        }

        if (confidence is double reportedConfidence)
        {
            DecisionContract.ValidateProbability(reportedConfidence, nameof(confidence));
        }

        Score = score;
        Confidence = confidence;
    }

    /// <summary>Gets the exact expected score reported by the service.</summary>
    public double Score { get; }

    /// <summary>Gets the reported probability for each zero-based level in request order.</summary>
    public IReadOnlyList<double> Probabilities { get; }

    /// <summary>Gets confidence when separately reported by the service.</summary>
    public double? Confidence { get; }
}
