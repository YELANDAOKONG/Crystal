using Crystal.Decisions.Questions;
using Crystal.Decisions.Results;
using Crystal.Internal;
using Crystal.Multimodal.Decisions;

namespace Crystal.Decisions;

/// <summary>Contains ordered question results or a whole-request block.</summary>
public sealed record DecisionResponse
{
    /// <summary>Initializes a response for the exact text or JSON request.</summary>
    public DecisionResponse(
        DecisionRequest request,
        IEnumerable<DecisionResult> results,
        TokenUsage? usage = null)
    {
        ArgumentNullException.ThrowIfNull(request);
        Results = SnapshotResults(request.Questions, results);
        Usage = usage;
    }

    /// <summary>Initializes a response for the exact multimodal request.</summary>
    public DecisionResponse(
        MultimodalDecisionRequest request,
        IEnumerable<DecisionResult> results,
        TokenUsage? usage = null)
    {
        ArgumentNullException.ThrowIfNull(request);
        Results = SnapshotResults(request.Questions, results);
        Usage = usage;
    }

    private DecisionResponse(DecisionBlock block, TokenUsage? usage)
    {
        ArgumentNullException.ThrowIfNull(block);
        Block = block;
        Results = [];
        Usage = usage;
    }

    /// <summary>Creates a blocked response with no question results.</summary>
    public static DecisionResponse Blocked(DecisionBlock block, TokenUsage? usage = null) =>
        new(block, usage);

    /// <summary>Gets one result per question in request order. Empty for a blocked request.</summary>
    public IReadOnlyList<DecisionResult> Results { get; }

    /// <summary>Gets the whole-request block when the request was blocked.</summary>
    public DecisionBlock? Block { get; }

    /// <summary>Gets provider-reported token usage when available.</summary>
    public TokenUsage? Usage { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(DecisionResponse);

    private static IReadOnlyList<DecisionResult> SnapshotResults<TContent>(
        IReadOnlyList<DecisionQuestion<TContent>> questions,
        IEnumerable<DecisionResult> results)
        where TContent : class
    {
        var snapshot = CollectionSnapshot.Create(results, nameof(results), allowEmpty: false);
        if (snapshot.Count != questions.Count)
        {
            throw new ArgumentException("A decision response requires one result per question.", nameof(results));
        }

        for (var index = 0; index < questions.Count; index++)
        {
            var question = questions[index];
            var result = snapshot[index];
            if (!string.Equals(question.Id, result.QuestionId, StringComparison.Ordinal)
                || !MatchesQuestion(question, result))
            {
                throw new ArgumentException(
                    "Decision results must match the question identifiers, types, and option or rubric order.",
                    nameof(results));
            }
        }

        return snapshot;
    }

    private static bool MatchesQuestion<TContent>(
        DecisionQuestion<TContent> question,
        DecisionResult result)
        where TContent : class =>
        (question, result) switch
        {
            (_, DecisionRefusal) => true,
            (PredicateQuestion<TContent>, PredicateResult) => true,
            (ChoiceQuestion<TContent> choice, ChoiceResult answer) =>
                choice.Options.Select(static option => option.Value)
                    .SequenceEqual(answer.Probabilities.Select(static probability => probability.Value)),
            (ScoreQuestion<TContent> score, ScoreResult answer) =>
                score.Levels.Count == answer.Probabilities.Count,
            _ => false
        };
}
