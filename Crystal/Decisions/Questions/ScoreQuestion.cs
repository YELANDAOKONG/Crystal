using Crystal.Internal;

namespace Crystal.Decisions.Questions;

/// <summary>Asks for a score against an ordered rubric of discrete levels.</summary>
/// <typeparam name="TContent">The content type of the decision profile.</typeparam>
public sealed record ScoreQuestion<TContent> : DecisionQuestion<TContent>
    where TContent : class
{
    /// <summary>Initializes a score question.</summary>
    public ScoreQuestion(
        string id,
        IEnumerable<ScoreLevel<TContent>> levels,
        IEnumerable<DecisionContent<TContent>>? instructions = null)
        : base(id, instructions)
    {
        Levels = CollectionSnapshot.Create(levels, nameof(levels), allowEmpty: false);
    }

    /// <inheritdoc />
    public override DecisionQuestionKind Kind => DecisionQuestionKind.Score;

    /// <summary>Gets the caller's rubric in order, with zero-based numeric values.</summary>
    public IReadOnlyList<ScoreLevel<TContent>> Levels { get; }
}
