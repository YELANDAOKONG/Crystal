namespace Crystal.Decisions;

/// <summary>Identifies where exact evidence or guidance appears in a decision request.</summary>
public enum DecisionContentPosition
{
    /// <summary>Evidence shared by every question.</summary>
    SharedEvidence,

    /// <summary>Instructions scoped to one question.</summary>
    QuestionInstructions,

    /// <summary>Criteria scoped to a true or false outcome.</summary>
    PredicateCriteria,

    /// <summary>Criteria scoped to one choice option.</summary>
    ChoiceCriteria,

    /// <summary>Criteria scoped to one ordered score level.</summary>
    ScoreCriteria
}
