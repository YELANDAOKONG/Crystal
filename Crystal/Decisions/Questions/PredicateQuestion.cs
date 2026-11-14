using Crystal.Internal;

namespace Crystal.Decisions.Questions;

/// <summary>Asks for the probability that a condition is true.</summary>
/// <typeparam name="TContent">The content type of the decision profile.</typeparam>
public sealed record PredicateQuestion<TContent> : DecisionQuestion<TContent>
    where TContent : class
{
    /// <summary>Initializes a predicate with optional outcome-specific criteria.</summary>
    public PredicateQuestion(
        string id,
        IEnumerable<DecisionContent<TContent>>? instructions = null,
        IEnumerable<DecisionContent<TContent>>? trueCriteria = null,
        IEnumerable<DecisionContent<TContent>>? falseCriteria = null)
        : base(id, instructions)
    {
        TrueCriteria = CollectionSnapshot.Create(trueCriteria ?? [], nameof(trueCriteria));
        FalseCriteria = CollectionSnapshot.Create(falseCriteria ?? [], nameof(falseCriteria));
        if (Instructions.Count == 0 && TrueCriteria.Count == 0 && FalseCriteria.Count == 0)
        {
            throw new ArgumentException("A predicate requires instructions or outcome criteria.", nameof(instructions));
        }
    }

    /// <inheritdoc />
    public override DecisionQuestionKind Kind => DecisionQuestionKind.Predicate;

    /// <summary>Gets exact ordered criteria for the true outcome.</summary>
    public IReadOnlyList<DecisionContent<TContent>> TrueCriteria { get; }

    /// <summary>Gets exact ordered criteria for the false outcome.</summary>
    public IReadOnlyList<DecisionContent<TContent>> FalseCriteria { get; }
}
