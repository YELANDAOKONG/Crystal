using Crystal.Chat;
using Crystal.Decisions.Questions;
using Crystal.Internal;

namespace Crystal.Decisions;

/// <summary>Evaluates ordered questions against exact non-media evidence.</summary>
public sealed record DecisionRequest
{
    /// <summary>Initializes a decision request with shared text or JSON blocks.</summary>
    public DecisionRequest(
        IEnumerable<DecisionContent<string>> evidence,
        IEnumerable<DecisionQuestion<string>> questions)
        : this(
            CollectionSnapshot.Create(evidence, nameof(evidence), allowEmpty: false),
            [],
            questions)
    {
    }

    private DecisionRequest(
        IReadOnlyList<DecisionContent<string>> evidence,
        IReadOnlyList<ChatItem> transcript,
        IEnumerable<DecisionQuestion<string>> questions)
    {
        Evidence = evidence;
        Transcript = transcript;
        Questions = DecisionContract.SnapshotQuestions(questions);
    }

    /// <summary>Creates a request that uses exact ordered Chat items as shared evidence.</summary>
    public static DecisionRequest FromTranscript(
        IEnumerable<ChatItem> transcript,
        IEnumerable<DecisionQuestion<string>> questions) =>
        new([], CollectionSnapshot.Create(transcript, nameof(transcript), allowEmpty: false), questions);

    /// <summary>Gets the ordered shared blocks. Empty when using a transcript.</summary>
    public IReadOnlyList<DecisionContent<string>> Evidence { get; }

    /// <summary>Gets the ordered Chat items. Empty when using shared blocks.</summary>
    public IReadOnlyList<ChatItem> Transcript { get; }

    /// <summary>Gets the exact questions in caller order.</summary>
    public IReadOnlyList<DecisionQuestion<string>> Questions { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(DecisionRequest);
}
