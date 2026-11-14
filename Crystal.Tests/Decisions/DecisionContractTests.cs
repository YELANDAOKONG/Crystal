using System.Text.Json;

using Crystal.Chat;
using Crystal.Decisions;
using Crystal.Decisions.Questions;
using Crystal.Decisions.Results;
using Crystal.Media;
using Crystal.Multimodal;
using Crystal.Multimodal.Decisions;
using Crystal.Tools;

namespace Crystal.Tests.Decisions;

public sealed class DecisionContractTests
{
    [Fact]
    public void TextRequest_ScopedContentAndJson_PreservesOrderAndOwnership()
    {
        DecisionContent<string> structured;
        using (var document = JsonDocument.Parse("""{"amount":12,"tags":["billing"]}"""))
        {
            structured = new DecisionContent<string>(document.RootElement);
        }

        var text = new DecisionContent<string>("  exact input  ");
        var evidence = new List<DecisionContent<string>> { text, structured };
        var instructions = new List<DecisionContent<string>> { structured, text };
        var predicate = new PredicateQuestion<string>("first", instructions, [text], [structured]);
        var questions = new List<DecisionQuestion<string>> { predicate };
        var request = new DecisionRequest(evidence, questions);
        evidence.Clear();
        instructions.Reverse();
        questions.Clear();

        Assert.Equal("  exact input  ", request.Evidence[0].Content);
        Assert.Same(structured, request.Evidence[1]);
        Assert.Equal(12, structured.Json!.Value.GetProperty("amount").GetInt32());
        Assert.Same(predicate, Assert.Single(request.Questions));
        Assert.Equal(new[] { structured, text }, predicate.Instructions);
        Assert.Same(text, Assert.Single(predicate.TrueCriteria));
        Assert.Same(structured, Assert.Single(predicate.FalseCriteria));
        Assert.Empty(request.Transcript);
        Assert.Equal(nameof(DecisionRequest), request.ToString());
        Assert.Equal("DecisionContent", structured.ToString());
    }

    [Fact]
    public void MultimodalRequest_MediaAndJsonAtDifferentPositions_PreservesEachExactBlock()
    {
        var sourceOpened = false;
        var source = new ReplayableStreamMediaSource(_ =>
        {
            sourceOpened = true;
            throw new InvalidOperationException();
        });
        var file = new FileContent(new FileMedia(source, new MediaMimeType("application/pdf"), "evidence.pdf"));
        var shared = new DecisionContent<MultimodalContent>(file);
        var reference = new DecisionContent<MultimodalContent>(
            new ImageContent(new ImageMedia(
                new UriMediaSource(new Uri("https://example.invalid/reference.png")),
                new MediaMimeType("image/png"))));
        using var document = JsonDocument.Parse("""["criterion",{"required":true}]""");
        var criteria = new DecisionContent<MultimodalContent>(document.RootElement);
        var level = new ScoreLevel<MultimodalContent>("exact label", [criteria, reference]);
        var question = new ScoreQuestion<MultimodalContent>("score", [level], [reference]);
        var request = new MultimodalDecisionRequest([shared], [question]);
        var result = new ScoreResult(question.Id, 0, [1]);
        var response = new DecisionResponse(request, [result]);

        Assert.Same(file, Assert.Single(request.Evidence).Content);
        Assert.Same(reference, Assert.Single(question.Instructions));
        Assert.Same(level, Assert.Single(question.Levels));
        Assert.Equal(new[] { criteria, reference }, level.Criteria);
        Assert.Same(result, Assert.Single(response.Results));
        Assert.False(sourceOpened);
    }

    [Fact]
    public void FromTranscript_CorrelatedToolHistory_PreservesExactChatItems()
    {
        var message = new ChatMessage(ChatRole.User, "Exact message");
        var call = new ToolCall("call", "lookup", "{}", ToolInputKind.Json);
        var result = new ToolResult("call", "Exact result");
        var items = new List<ChatItem> { message, call, result };
        var request = DecisionRequest.FromTranscript(items, [CreatePredicate()]);
        items.Clear();

        Assert.Empty(request.Evidence);
        Assert.Same(message, request.Transcript[0]);
        Assert.Same(call, request.Transcript[1]);
        Assert.Same(result, request.Transcript[2]);
        Assert.Equal(call.CallId, result.CallId);
    }

    [Fact]
    public void DecisionResponse_MixedResults_PreservesTypedValuesAndReportedNumbers()
    {
        var predicate = CreatePredicate();
        var textChoice = new ChoiceValue("true");
        var booleanChoice = new ChoiceValue(true);
        var choice = new ChoiceQuestion<string>("choice", [new(textChoice), new(booleanChoice)]);
        var score = new ScoreQuestion<string>("score", [new("Low"), new("High")]);
        var request = new DecisionRequest([new("input")], [predicate, choice, score]);
        var refusal = new DecisionRefusal(predicate.Id);
        var selection = new ChoiceResult(choice.Id, booleanChoice,
            [new(textChoice, 0.2001), new(booleanChoice, 0.8)], confidence: 0.7);
        var rating = new ScoreResult(score.Id, 0.432, [0.6, 0.4], confidence: 0.5);
        var usage = new TokenUsage(12, 3);
        var results = new List<DecisionResult> { refusal, selection, rating };
        var response = new DecisionResponse(request, results, usage);
        results.Clear();

        Assert.NotEqual(textChoice, booleanChoice);
        Assert.Null(response.Block);
        Assert.Same(usage, response.Usage);
        Assert.Same(refusal, response.Results[0]);
        Assert.Same(selection, response.Results[1]);
        Assert.Equal(0.2001, selection.Probabilities[0].Probability);
        Assert.Equal(0.8, selection.Probabilities[1].Probability);
        Assert.Equal(0.7, selection.Confidence);
        Assert.Same(rating, response.Results[2]);
        Assert.Equal(0.432, rating.Score);
        Assert.Equal(0.5, rating.Confidence);
    }

    [Fact]
    public void DecisionResponse_MismatchedResults_RejectsBrokenProviderMapping()
    {
        var predicate = CreatePredicate();
        var yes = new ChoiceValue("yes");
        var no = new ChoiceValue("no");
        var choice = new ChoiceQuestion<string>("choice", [new(yes), new(no)]);
        var score = new ScoreQuestion<string>("score", [new("Low"), new("High")]);
        var request = new DecisionRequest([new("input")], [predicate, choice, score]);
        var predicateAnswer = new PredicateResult(predicate.Id, 0.75);
        var choiceAnswer = new ChoiceResult(choice.Id, yes, [new(yes, 0.75), new(no, 0.25)]);
        var scoreAnswer = new ScoreResult(score.Id, 0.4, [0.6, 0.4]);
        var wrongType = new PredicateResult(choice.Id, 0.75);
        var wrongOptionOrder = new ChoiceResult(choice.Id, yes, [new(no, 0.25), new(yes, 0.75)]);
        var wrongLevelCount = new ScoreResult(score.Id, 0.4, [1]);
        DecisionResult[][] invalidResults =
        [
            [predicateAnswer],
            [choiceAnswer, predicateAnswer, scoreAnswer],
            [predicateAnswer, wrongType, scoreAnswer],
            [predicateAnswer, wrongOptionOrder, scoreAnswer],
            [predicateAnswer, choiceAnswer, wrongLevelCount]
        ];

        foreach (var results in invalidResults)
        {
            Assert.Equal("results", Assert.Throws<ArgumentException>(
                () => new DecisionResponse(request, results)).ParamName);
        }
    }

    [Fact]
    public void Blocked_WholeRequestBlock_HasNoQuestionResults()
    {
        var block = new DecisionBlock("adapter-reason", "private detail");
        var response = DecisionResponse.Blocked(block);

        Assert.Same(block, response.Block);
        Assert.Empty(response.Results);
        Assert.Equal(nameof(DecisionBlock), block.ToString());
        Assert.Equal(nameof(DecisionResponse), response.ToString());
    }

    [Fact]
    public void DecisionConstructors_InvalidShapes_RejectWithoutDisclosingContent()
    {
        var predicate = CreatePredicate();
        using var scalar = JsonDocument.Parse("true");
        var repeated = new ChoiceValue("private option");
        var duplicateChoices = Assert.Throws<ArgumentException>(() =>
            new ChoiceQuestion<string>("choice", [new(repeated), new(repeated)]));

        Assert.DoesNotContain("private option", duplicateChoices.Message);
        Assert.Throws<ArgumentException>(() => new DecisionContent<string>(scalar.RootElement));
        Assert.Throws<ArgumentException>(() => new DecisionRequest([new("input")], [predicate, predicate]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PredicateResult(predicate.Id, double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PredicateResult(predicate.Id, 0.5, double.PositiveInfinity));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ScoreResult("score", double.NegativeInfinity, [1]));
    }

    [Fact]
    public void Capabilities_PositionSpecificShapes_DistinguishSharedAndQuestionContent()
    {
        var sharedJson = new DecisionDataCapability(DecisionContentPosition.SharedEvidence, DecisionDataKind.Json);
        var questionJson = new DecisionDataCapability(DecisionContentPosition.QuestionInstructions, DecisionDataKind.Json);
        var basic = new DecisionCapabilities([DecisionQuestionKind.Predicate], [sharedJson, questionJson]);
        var file = new MultimodalContentCapability(ContentModality.File, [MediaSourceKind.Uri]);
        var sharedFile = new DecisionMediaCapability(DecisionContentPosition.SharedEvidence, file);
        var questionFile = new DecisionMediaCapability(DecisionContentPosition.QuestionInstructions, file);
        var capabilities = new MultimodalDecisionCapabilities(basic, [sharedFile, questionFile]);

        Assert.Same(basic, capabilities.Decisions);
        Assert.Equal(new[] { sharedJson, questionJson }, basic.Data);
        Assert.Equal(new[] { sharedFile, questionFile }, capabilities.Media);
        Assert.Throws<ArgumentException>(() => new MultimodalDecisionCapabilities(basic, [sharedFile, sharedFile]));
    }

    private static PredicateQuestion<string> CreatePredicate() =>
        new("predicate", [new("Exact question")]);
}
