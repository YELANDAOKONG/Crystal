namespace Crystal.Decisions.Results;

/// <summary>Represents the result for one question.</summary>
public abstract record DecisionResult
{
    private protected DecisionResult(string questionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(questionId);
        QuestionId = questionId;
    }

    /// <summary>Gets the exact identifier of the corresponding question.</summary>
    public string QuestionId { get; }

    /// <inheritdoc />
    public sealed override string ToString() => GetType().Name;
}
