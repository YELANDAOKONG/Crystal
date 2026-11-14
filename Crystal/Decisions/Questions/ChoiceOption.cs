using Crystal.Internal;

namespace Crystal.Decisions.Questions;

/// <summary>Identifies one allowed choice and its optional exact criteria.</summary>
/// <typeparam name="TContent">The content type of the decision profile.</typeparam>
public sealed record ChoiceOption<TContent>
    where TContent : class
{
    /// <summary>Initializes a closed-choice option.</summary>
    public ChoiceOption(ChoiceValue value, IEnumerable<DecisionContent<TContent>>? criteria = null)
    {
        ArgumentNullException.ThrowIfNull(value);
        Value = value;
        Criteria = CollectionSnapshot.Create(criteria ?? [], nameof(criteria));
    }

    /// <summary>Gets the exact value used to identify the option.</summary>
    public ChoiceValue Value { get; }

    /// <summary>Gets the ordered criteria for this option.</summary>
    public IReadOnlyList<DecisionContent<TContent>> Criteria { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(ChoiceOption<TContent>);
}
