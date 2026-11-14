using Crystal.Internal;

namespace Crystal.Decisions.Questions;

/// <summary>Describes one level in an ordered scoring rubric.</summary>
/// <typeparam name="TContent">The content type of the decision profile.</typeparam>
public sealed record ScoreLevel<TContent>
    where TContent : class
{
    /// <summary>Initializes one level with a label, criteria, or both.</summary>
    public ScoreLevel(string? label = null, IEnumerable<DecisionContent<TContent>>? criteria = null)
    {
        Criteria = CollectionSnapshot.Create(criteria ?? [], nameof(criteria));
        if (label is null && Criteria.Count == 0)
        {
            throw new ArgumentException("A score level requires a label or criteria.", nameof(label));
        }

        Label = label;
    }

    /// <summary>Gets the exact optional level label.</summary>
    public string? Label { get; }

    /// <summary>Gets exact ordered criteria scoped to this level.</summary>
    public IReadOnlyList<DecisionContent<TContent>> Criteria { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(ScoreLevel<TContent>);
}
