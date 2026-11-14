namespace Crystal.Decisions;

/// <summary>Identifies an available portable decision question type.</summary>
public enum DecisionQuestionKind
{
    /// <summary>Estimate the probability that a condition is true.</summary>
    Predicate,

    /// <summary>Select one value from caller-supplied options.</summary>
    Choice,

    /// <summary>Score evidence against an ordered rubric.</summary>
    Score
}
