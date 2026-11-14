using Crystal.Internal;

namespace Crystal.Decisions.Questions;

/// <summary>Represents one named question evaluated against shared evidence.</summary>
/// <typeparam name="TContent">The content type of the decision profile.</typeparam>
public abstract record DecisionQuestion<TContent>
    where TContent : class
{
    private protected DecisionQuestion(
        string id,
        IEnumerable<DecisionContent<TContent>>? instructions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        Id = id;
        Instructions = CollectionSnapshot.Create(instructions ?? [], nameof(instructions));
    }

    /// <summary>Gets the caller-assigned, case-sensitive identifier.</summary>
    public string Id { get; }

    /// <summary>Gets the portable question type.</summary>
    public abstract DecisionQuestionKind Kind { get; }

    /// <summary>Gets exact ordered instructions scoped to this question.</summary>
    public IReadOnlyList<DecisionContent<TContent>> Instructions { get; }

    /// <inheritdoc />
    public sealed override string ToString() => GetType().Name;
}
