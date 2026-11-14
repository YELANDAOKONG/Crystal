using Crystal.Decisions;
using Crystal.Decisions.Questions;
using Crystal.Internal;
using Crystal.Multimodal.Chat;

namespace Crystal.Multimodal.Decisions;

/// <summary>Evaluates ordered questions against exact typed evidence.</summary>
public sealed record MultimodalDecisionRequest
{
    /// <summary>Initializes a decision request with shared multimodal or JSON blocks.</summary>
    public MultimodalDecisionRequest(
        IEnumerable<DecisionContent<MultimodalContent>> evidence,
        IEnumerable<DecisionQuestion<MultimodalContent>> questions)
        : this(
            CollectionSnapshot.Create(evidence, nameof(evidence), allowEmpty: false),
            [],
            questions)
    {
    }

    private MultimodalDecisionRequest(
        IReadOnlyList<DecisionContent<MultimodalContent>> evidence,
        IReadOnlyList<MultimodalChatItem> transcript,
        IEnumerable<DecisionQuestion<MultimodalContent>> questions)
    {
        Evidence = evidence;
        Transcript = transcript;
        Questions = DecisionContract.SnapshotQuestions(questions);
    }

    /// <summary>Creates a request that uses exact ordered multimodal Chat items as shared evidence.</summary>
    public static MultimodalDecisionRequest FromTranscript(
        IEnumerable<MultimodalChatItem> transcript,
        IEnumerable<DecisionQuestion<MultimodalContent>> questions) =>
        new([], CollectionSnapshot.Create(transcript, nameof(transcript), allowEmpty: false), questions);

    /// <summary>Gets the ordered shared blocks. Empty when using a transcript.</summary>
    public IReadOnlyList<DecisionContent<MultimodalContent>> Evidence { get; }

    /// <summary>Gets the ordered Chat items. Empty when using shared blocks.</summary>
    public IReadOnlyList<MultimodalChatItem> Transcript { get; }

    /// <summary>Gets the exact questions in caller order.</summary>
    public IReadOnlyList<DecisionQuestion<MultimodalContent>> Questions { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(MultimodalDecisionRequest);
}
