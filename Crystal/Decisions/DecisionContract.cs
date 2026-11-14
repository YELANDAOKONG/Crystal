using Crystal.Decisions.Questions;
using Crystal.Internal;

namespace Crystal.Decisions;

internal static class DecisionContract
{
    public static IReadOnlyList<DecisionQuestion<TContent>> SnapshotQuestions<TContent>(
        IEnumerable<DecisionQuestion<TContent>> questions)
        where TContent : class
    {
        var snapshot = CollectionSnapshot.Create(questions, nameof(questions), allowEmpty: false);
        if (snapshot.Select(static question => question.Id).Distinct(StringComparer.Ordinal).Count()
            != snapshot.Count)
        {
            throw new ArgumentException("Decision question identifiers must be unique.", nameof(questions));
        }

        return snapshot;
    }

    public static void ValidateProbability(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value < 0 || value > 1)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Probability and confidence must be between 0 and 1.");
        }
    }
}
