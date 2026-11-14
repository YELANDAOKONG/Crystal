namespace Crystal.Decisions.Results;

/// <summary>Reports that the service declined one question.</summary>
public sealed record DecisionRefusal : DecisionResult
{
    /// <summary>Initializes a refusal for the specified question.</summary>
    public DecisionRefusal(string questionId)
        : base(questionId)
    {
    }
}
