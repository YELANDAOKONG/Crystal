using System.Runtime.CompilerServices;
using System.Text.Json;

using Crystal;
using Crystal.Media;
using Crystal.Multimodal;
using Crystal.Multimodal.Agents;
using Crystal.Multimodal.Chat;
using Crystal.Multimodal.Tools;
using Crystal.Reasoning;
using Crystal.Tools;

namespace Crystal.Tests;

public sealed class MultimodalAgentStreamingTests
{
    [Fact]
    public async Task StreamPreservesTypedMediaReasoningAndEventOrder()
    {
        var mediaOpenCount = 0;
        var inputSource = new ReplayableStreamMediaSource(_ =>
        {
            mediaOpenCount++;
            return ValueTask.FromResult<Stream>(new MemoryStream([1, 2]));
        });
        var inputImage = new ImageContent(
            new ImageMedia(inputSource, new MediaMimeType("image/png")));
        var initial = new MultimodalMessage(
            MultimodalChatRole.User,
            [new TextContent("question"), inputImage]);
        var outputSource = new UriMediaSource(new Uri("https://example.invalid/image"));
        var outputImage = new ImageContent(
            new ImageMedia(outputSource, new MediaMimeType("image/png")));
        var reasoningState = new OpaqueReasoningState("test", new byte[] { 3, 4 });
        var usage = new TokenUsage(5, 7, 2);
        MultimodalChatStreamEvent[] providerEvents =
        [
            new MultimodalMessageStarted(1, 0, MultimodalChatRole.Assistant),
            new MultimodalMessageTextDelta(1, 0, 0, "other"),
            new MultimodalChatCandidateCompleted(1, FinishReason.Stop),
            new MultimodalReasoningContentReceived(
                0, 0, 0, MultimodalReasoningKind.Summary, new TextContent("reason")),
            new MultimodalReasoningStateReceived(0, 0, reasoningState),
            new MultimodalMessageStarted(0, 1, MultimodalChatRole.Assistant),
            new MultimodalMessageTextDelta(0, 1, 0, "look"),
            new MultimodalMessageContentReceived(0, 1, 1, outputImage),
            new MultimodalChatCandidateCompleted(0, FinishReason.Stop),
            new MultimodalChatUsageReceived(usage)
        ];
        var capabilities = CreateCapabilities();
        var client = new RecordingStreamingClient(capabilities, providerEvents);
        MultimodalChatResponse? selectedResponse = null;
        var agent = new MultimodalAgent(client, (response, _) =>
        {
            selectedResponse = response;
            return ValueTask.FromResult(0);
        });
        var request = new MultimodalAgentRunRequest(
            Guid.NewGuid(),
            [initial],
            new MultimodalAgentRunLimits(1, 0, TimeSpan.FromMinutes(1)));
        var runEvents = new List<MultimodalAgentRunEvent>();

        await foreach (var runEvent in agent.StreamAsync(request))
        {
            runEvents.Add(runEvent);
        }

        var forwarded = runEvents.OfType<MultimodalAgentModelStreamEvent>().ToArray();
        Assert.Equal(providerEvents.Length, forwarded.Length);
        for (var index = 0; index < providerEvents.Length; index++)
        {
            Assert.Same(providerEvents[index], forwarded[index].StreamEvent);
        }

        Assert.Same(capabilities, agent.Capabilities);
        Assert.Same(initial, Assert.Single(client.Request!.Items));
        Assert.Same(
            selectedResponse,
            Assert.Single(runEvents.OfType<MultimodalAgentModelResponseEvent>()).Response);
        Assert.False(client.CompleteCalled);
        Assert.Equal(0, mediaOpenCount);

        var response = Assert.IsType<MultimodalChatResponse>(selectedResponse);
        Assert.Equal(2, response.Candidates.Count);
        var reasoning = Assert.IsType<MultimodalReasoningItem>(
            response.Candidates[0].Items[0]);
        Assert.Same(reasoningState, reasoning.Content.State);
        Assert.Equal(MultimodalReasoningKind.Summary, reasoning.Content.Parts[0].Kind);
        Assert.Equal(
            "reason",
            Assert.IsType<TextContent>(reasoning.Content.Parts[0].Content).Text);
        var answer = Assert.IsType<MultimodalMessage>(response.Candidates[0].Items[1]);
        Assert.Equal("look", Assert.IsType<TextContent>(answer.Contents[0]).Text);
        Assert.Same(outputImage, answer.Contents[1]);
        Assert.Equal(
            "other",
            Assert.IsType<TextContent>(
                Assert.IsType<MultimodalMessage>(response.Candidates[1].Items[0])
                    .Contents[0]).Text);

        var result = Assert.Single(runEvents.OfType<MultimodalAgentRunCompletedEvent>()).Result;
        Assert.Equal(MultimodalAgentRunStopReason.Completed, result.StopReason);
        Assert.Equal(1, result.ModelCallCount);
        Assert.Same(initial, result.Transcript[0]);
        Assert.Same(reasoning, result.Transcript[1]);
        Assert.Same(answer, result.Transcript[2]);
        Assert.Equal(usage.InputTokenCount, result.Usage!.InputTokenCount);
        Assert.Equal(usage.OutputTokenCount, result.Usage.OutputTokenCount);
    }

    [Fact]
    public async Task IncompleteStreamFailsWithoutSelectingOrAddingContent()
    {
        MultimodalChatStreamEvent[] providerEvents =
        [
            new MultimodalMessageStarted(0, 0, MultimodalChatRole.Assistant),
            new MultimodalMessageTextDelta(0, 0, 0, "private partial output")
        ];
        var client = new RecordingStreamingClient(CreateCapabilities(), providerEvents);
        var selectorCalled = false;
        var agent = new MultimodalAgent(client, (_, _) =>
        {
            selectorCalled = true;
            return ValueTask.FromResult(0);
        });
        var request = new MultimodalAgentRunRequest(
            Guid.NewGuid(),
            [new MultimodalMessage(MultimodalChatRole.User, [new TextContent("question")])],
            new MultimodalAgentRunLimits(1, 0, TimeSpan.FromMinutes(1)));

        var failure = await Assert.ThrowsAsync<InvalidOperationException>(
            () => agent.RunAsync(request));

        Assert.Contains("before a candidate completed", failure.Message);
        Assert.DoesNotContain("private partial output", failure.Message);
        Assert.False(selectorCalled);
    }

    [Fact]
    public async Task StreamedToolCallAndTypedResultAreReplayedExactly()
    {
        var callImage = new ImageContent(new ImageMedia(
            new InlineMediaSource(new byte[] { 1 }),
            new MediaMimeType("image/png")));
        var resultImage = new ImageContent(new ImageMedia(
            new InlineMediaSource(new byte[] { 2 }),
            new MediaMimeType("image/png")));
        var client = new ToolCallingStreamingClient(callImage);
        var executor = new RecordingMultimodalToolExecutor(resultImage);
        var agent = new MultimodalAgent(
            client,
            (_, _) => ValueTask.FromResult(0),
            executor);
        var initial = new MultimodalMessage(
            MultimodalChatRole.User,
            [new TextContent("question")]);
        var request = new MultimodalAgentRunRequest(
            Guid.NewGuid(),
            [initial],
            new MultimodalAgentRunLimits(2, 1, TimeSpan.FromMinutes(1)));

        var result = await agent.RunAsync(request);

        Assert.Equal(2, result.ModelCallCount);
        Assert.Equal(1, result.ToolCallCount);
        Assert.Equal(2, client.Requests.Count);
        Assert.Same(initial, Assert.Single(client.Requests[0].Items));
        Assert.Same(executor.Definitions[0], Assert.Single(client.Requests[0].Tools));
        var replayedCall = Assert.IsType<MultimodalToolCall>(client.Requests[1].Items[1]);
        Assert.Same(executor.Call, replayedCall);
        Assert.Equal("call-1", replayedCall.CallId);
        Assert.Equal("lookup", replayedCall.Name);
        Assert.Equal("{\"q\":\"x\"}", replayedCall.Arguments);
        Assert.Same(callImage, Assert.Single(replayedCall.Contents));
        var replayedResult = Assert.IsType<MultimodalToolResult>(client.Requests[1].Items[2]);
        Assert.Same(executor.Result, replayedResult);
        Assert.Equal("call-1", replayedResult.CallId);
        Assert.Equal("done", Assert.IsType<TextContent>(replayedResult.Contents[0]).Text);
        Assert.Same(resultImage, replayedResult.Contents[1]);
        Assert.Same(initial, result.Transcript[0]);
        Assert.Same(replayedCall, result.Transcript[1]);
        Assert.Same(replayedResult, result.Transcript[2]);
        Assert.Equal(
            "answer",
            Assert.IsType<TextContent>(
                Assert.IsType<MultimodalMessage>(result.Transcript[3]).Contents[0]).Text);
    }

    private static MultimodalChatCapabilities CreateCapabilities(
        bool supportsTools = false)
    {
        var text = new MultimodalContentCapability(ContentModality.Text);
        var image = new MultimodalContentCapability(
            ContentModality.Image,
            [MediaSourceKind.ReplayableStream, MediaSourceKind.Uri]);
        return new MultimodalChatCapabilities(
            [text, image],
            [text, image],
            supportsTools);
    }

    private sealed class RecordingStreamingClient(
        MultimodalChatCapabilities capabilities,
        IReadOnlyList<MultimodalChatStreamEvent> events)
        : IStreamingMultimodalChatClient
    {
        public MultimodalChatCapabilities Capabilities => capabilities;

        public MultimodalChatRequest? Request { get; private set; }

        public bool CompleteCalled { get; private set; }

        public Task<MultimodalChatResponse> CompleteAsync(
            MultimodalChatRequest request,
            CancellationToken cancellationToken = default)
        {
            CompleteCalled = true;
            throw new InvalidOperationException("The non-streaming path was called.");
        }

        public async IAsyncEnumerable<MultimodalChatStreamEvent> StreamAsync(
            MultimodalChatRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            Request = request;
            foreach (var streamEvent in events)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return streamEvent;
                await Task.Yield();
            }
        }
    }

    private sealed class ToolCallingStreamingClient(ImageContent callImage)
        : IStreamingMultimodalChatClient
    {
        public MultimodalChatCapabilities Capabilities { get; } =
            CreateCapabilities(supportsTools: true);

        public List<MultimodalChatRequest> Requests { get; } = [];

        public Task<MultimodalChatResponse> CompleteAsync(
            MultimodalChatRequest request,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("The non-streaming path was called.");

        public async IAsyncEnumerable<MultimodalChatStreamEvent> StreamAsync(
            MultimodalChatRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            if (Requests.Count == 1)
            {
                yield return new MultimodalToolCallDelta(0, 0, "call", "look", "{\"q\":");
                yield return new MultimodalToolCallContentReceived(0, 0, 0, callImage);
                yield return new MultimodalToolCallDelta(0, 0, "-1", "up", "\"x\"}");
                yield return new MultimodalChatCandidateCompleted(0, FinishReason.ToolCalls);
            }
            else
            {
                yield return new MultimodalMessageStarted(0, 0, MultimodalChatRole.Assistant);
                yield return new MultimodalMessageContentReceived(
                    0, 0, 0, new TextContent("answer"));
                yield return new MultimodalChatCandidateCompleted(0, FinishReason.Stop);
            }

            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    private sealed class RecordingMultimodalToolExecutor : IMultimodalToolExecutor
    {
        private readonly ImageContent _resultImage;

        public RecordingMultimodalToolExecutor(ImageContent resultImage)
        {
            _resultImage = resultImage;
            using var schema = JsonDocument.Parse("{}");
            Definitions = [new ToolDefinition("lookup", schema.RootElement)];
        }

        public IReadOnlyList<ToolDefinition> Definitions { get; }

        public MultimodalToolCall? Call { get; private set; }

        public MultimodalToolResult? Result { get; private set; }

        public Task<IReadOnlyList<MultimodalToolResult>> ExecuteAsync(
            IEnumerable<MultimodalToolCall> calls,
            CancellationToken cancellationToken = default)
        {
            Call = Assert.Single(calls);
            Result = new MultimodalToolResult(
                Call.CallId,
                [new TextContent("done"), _resultImage]);
            IReadOnlyList<MultimodalToolResult> results = [Result];
            return Task.FromResult(results);
        }
    }
}
