using Crystal.Internal;

namespace Crystal.Decisions.Questions;

/// <summary>Asks the client to select one of the supplied choices.</summary>
/// <typeparam name="TContent">The content type of the decision profile.</typeparam>
public sealed record ChoiceQuestion<TContent> : DecisionQuestion<TContent>
    where TContent : class
{
    /// <summary>Initializes a choice question.</summary>
    public ChoiceQuestion(
        string id,
        IEnumerable<ChoiceOption<TContent>> options,
        IEnumerable<DecisionContent<TContent>>? instructions = null)
        : base(id, instructions)
    {
        Options = CollectionSnapshot.Create(options, nameof(options), allowEmpty: false);
        if (Options.Select(static option => option.Value).Distinct().Count() != Options.Count)
        {
            throw new ArgumentException("Decision choice values must be unique.", nameof(options));
        }
    }

    /// <inheritdoc />
    public override DecisionQuestionKind Kind => DecisionQuestionKind.Choice;

    /// <summary>Gets the allowed choices in caller order.</summary>
    public IReadOnlyList<ChoiceOption<TContent>> Options { get; }
}
