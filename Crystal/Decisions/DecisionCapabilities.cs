using Crystal.Internal;

namespace Crystal.Decisions;

/// <summary>Declares individual decision question and non-media input shapes.</summary>
public sealed record DecisionCapabilities
{
    /// <summary>Initializes decision capabilities.</summary>
    /// <param name="questionKinds">The non-empty supported question types.</param>
    /// <param name="data">The supported position and data-shape pairs.</param>
    /// <param name="supportsTranscript">Whether ordered Chat evidence is accepted.</param>
    /// <param name="supportsBooleanChoices">Whether choice values may be booleans.</param>
    public DecisionCapabilities(
        IEnumerable<DecisionQuestionKind> questionKinds,
        IEnumerable<DecisionDataCapability> data,
        bool supportsTranscript = false,
        bool supportsBooleanChoices = false)
    {
        QuestionKinds = CollectionSnapshot.Create(questionKinds, nameof(questionKinds), allowEmpty: false);
        if (QuestionKinds.Any(static kind => !Enum.IsDefined(kind))
            || QuestionKinds.Distinct().Count() != QuestionKinds.Count)
        {
            throw new ArgumentException("Decision question kinds must be defined and unique.", nameof(questionKinds));
        }

        Data = CollectionSnapshot.Create(data, nameof(data));
        if (Data.Distinct().Count() != Data.Count)
        {
            throw new ArgumentException("Decision data capability pairs must be unique.", nameof(data));
        }

        if (supportsBooleanChoices && !QuestionKinds.Contains(DecisionQuestionKind.Choice))
        {
            throw new ArgumentException("Boolean values require support for choice questions.", nameof(supportsBooleanChoices));
        }

        SupportsTranscript = supportsTranscript;
        SupportsBooleanChoices = supportsBooleanChoices;
    }

    /// <summary>Gets the supported question types.</summary>
    public IReadOnlyList<DecisionQuestionKind> QuestionKinds { get; }

    /// <summary>Gets the supported non-media content shapes by position.</summary>
    public IReadOnlyList<DecisionDataCapability> Data { get; }

    /// <summary>Gets whether ordered Chat transcript evidence is accepted.</summary>
    public bool SupportsTranscript { get; }

    /// <summary>Gets whether closed-choice values may be booleans.</summary>
    public bool SupportsBooleanChoices { get; }
}
