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
        var inputFile = new FileContent(
            new FileMedia(inputSource, new MediaMimeType("application/pdf")));
        var initial = new MultimodalMessage(
            MultimodalChatRole.User,
            [new TextContent("question"), inputImage, inputFile]);
        var outputImage = new ImageContent(
            new ImageMedia(
                new UriMediaSource(new Uri("https://example.invalid/image")),
                new MediaMimeType("image/png")));
        var outputFile = new FileContent(new FileMedia(
            new UriMediaSource(new Uri("https://example.invalid/report")),
            new MediaMimeType("application/pdf"),
            "report.pdf"));
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
            new MultimodalMessageContentReceived(0, 1, 2, outputFile),
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
        Assert.Same(outputFile, answer.Contents[2]);
        Assert.Same(ContentModality.File, outputFile.Modality);
        Assert.Equal("report.pdf", outputFile.File.FileName);
        Assert.Equal(nameof(FileMedia), outputFile.File.ToString());
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
        var callFile = new FileContent(new FileMedia(
            new InlineMediaSource(new byte[] { 1 }),
            new MediaMimeType("application/pdf")));
        var resultFile = new FileContent(new FileMedia(
            new InlineMediaSource(new byte[] { 2 }),
            new MediaMimeType("application/pdf"),
            "result.pdf"));
        var client = new ToolCallingStreamingClient(callFile);
        var executor = new RecordingMultimodalToolExecutor(resultFile);
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
        Assert.Same(callFile, Assert.Single(replayedCall.Contents));
        var replayedResult = Assert.IsType<MultimodalToolResult>(client.Requests[1].Items[2]);
        Assert.Same(executor.Result, replayedResult);
        Assert.Equal("call-1", replayedResult.CallId);
        Assert.Equal("done", Assert.IsType<TextContent>(replayedResult.Contents[0]).Text);
        Assert.Same(resultFile, replayedResult.Contents[1]);
        Assert.Same(initial, result.Transcript[0]);
        Assert.Same(replayedCall, result.Transcript[1]);
        Assert.Same(replayedResult, result.Transcript[2]);
        Assert.Equal(
            "answer",
            Assert.IsType<TextContent>(
                Assert.IsType<MultimodalMessage>(result.Transcript[3]).Contents[0]).Text);
    }

    [Fact]
    public async Task StreamedTextToolInputIsReplayedExactly()
    {
        var client = new TextToolCallingStreamingClient();
        var executor = new RecordingTextMultimodalToolExecutor();
        var agent = new MultimodalAgent(
            client,
            (_, _) => ValueTask.FromResult(0),
            executor);
        var initial = new MultimodalMessage(
            MultimodalChatRole.User,
            [new TextContent("capture some text")]);
        var request = new MultimodalAgentRunRequest(
            Guid.NewGuid(),
            [initial],
            new MultimodalAgentRunLimits(2, 1, TimeSpan.FromMinutes(1)));

        var result = await agent.RunAsync(request);

        Assert.Equal(2, result.ModelCallCount);
        Assert.Equal(1, result.ToolCallCount);
        Assert.Same(
            executor.Definitions[0],
            Assert.Single(client.Requests[0].Tools));
        var replayedCall = Assert.IsType<MultimodalToolCall>(
            client.Requests[1].Items[1]);
        Assert.Same(executor.Call, replayedCall);
        Assert.Equal(ToolInputKind.Text, replayedCall.InputKind);
        Assert.Equal(
            "first line\nsecond line",
            replayedCall.Arguments);
        Assert.Same(executor.Result, client.Requests[1].Items[2]);
    }

    [Fact]
    public async Task UnlimitedLimitsAllowMultipleMultimodalModelAndToolAttempts()
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
        var request = new MultimodalAgentRunRequest(
            Guid.NewGuid(),
            [new MultimodalMessage(
                MultimodalChatRole.User,
                [new TextContent("question")])],
            MultimodalAgentRunLimits.Unlimited);

        var result = await agent.RunAsync(request);

        Assert.Equal(MultimodalAgentRunStopReason.Completed, result.StopReason);
        Assert.Equal(2, result.ModelCallCount);
        Assert.Equal(1, result.ToolCallCount);
        Assert.Equal(2, client.Requests.Count);
    }

    [Fact]
    public async Task UnlimitedMultimodalModelCallsStillHonorFiniteToolCallLimit()
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
        var request = new MultimodalAgentRunRequest(
            Guid.NewGuid(),
            [new MultimodalMessage(
                MultimodalChatRole.User,
                [new TextContent("question")])],
            new MultimodalAgentRunLimits(null, 0, null));

        var result = await agent.RunAsync(request);

        Assert.Equal(MultimodalAgentRunStopReason.ToolCallLimitReached, result.StopReason);
        Assert.Equal(1, result.ModelCallCount);
        Assert.Equal(0, result.ToolCallCount);
        Assert.Single(client.Requests);
        Assert.Null(executor.Call);
    }

    private static MultimodalChatCapabilities CreateCapabilities(
        bool supportsTools = false)
    {
        var text = new MultimodalContentCapability(ContentModality.Text);
        var image = new MultimodalContentCapability(
            ContentModality.Image,
            [MediaSourceKind.Inline, MediaSourceKind.ReplayableStream, MediaSourceKind.Uri]);
        var file = new MultimodalContentCapability(
            ContentModality.File,
            [MediaSourceKind.Inline, MediaSourceKind.ReplayableStream, MediaSourceKind.Uri]);
        return new MultimodalChatCapabilities(
            [text, image, file],
            [text, image, file],
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

    private sealed class ToolCallingStreamingClient(MultimodalContent callContent)
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
                yield return new MultimodalToolCallDelta(
                    0, 0, "call", "look", "{\"q\":", ToolInputKind.Json);
                yield return new MultimodalToolCallContentReceived(0, 0, 0, callContent);
                yield return new MultimodalToolCallDelta(
                    0, 0, "-1", "up", "\"x\"}", ToolInputKind.Json);
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

    private sealed class TextToolCallingStreamingClient
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
                yield return new MultimodalToolCallDelta(
                    0,
                    0,
                    "call",
                    "capture",
                    "first line\n",
                    ToolInputKind.Text);
                yield return new MultimodalToolCallDelta(
                    0,
                    0,
                    "",
                    "",
                    "second line",
                    ToolInputKind.Text);
                yield return new MultimodalChatCandidateCompleted(
                    0,
                    FinishReason.ToolCalls);
            }
            else
            {
                yield return new MultimodalMessageStarted(
                    0,
                    0,
                    MultimodalChatRole.Assistant);
                yield return new MultimodalMessageContentReceived(
                    0,
                    0,
                    0,
                    new TextContent("answer"));
                yield return new MultimodalChatCandidateCompleted(
                    0,
                    FinishReason.Stop);
            }

            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    private sealed class RecordingMultimodalToolExecutor : IMultimodalToolExecutor
    {
        private readonly MultimodalContent _resultContent;

        public RecordingMultimodalToolExecutor(MultimodalContent resultContent)
        {
            _resultContent = resultContent;
            using var schema = JsonDocument.Parse("{}");
            Definitions = [ToolDefinition.Json("lookup", schema.RootElement)];
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
                [new TextContent("done"), _resultContent]);
            IReadOnlyList<MultimodalToolResult> results = [Result];
            return Task.FromResult(results);
        }
    }

    private sealed class RecordingTextMultimodalToolExecutor
        : IMultimodalToolExecutor
    {
        public RecordingTextMultimodalToolExecutor()
        {
            Definitions =
                [ToolDefinition.Text("capture", "Captures text input.")];
        }

        public IReadOnlyList<ToolDefinition> Definitions { get; }

        public MultimodalToolCall? Call { get; private set; }

        public MultimodalToolResult? Result { get; private set; }

        public Task<IReadOnlyList<MultimodalToolResult>> ExecuteAsync(
            IEnumerable<MultimodalToolCall> calls,
            CancellationToken cancellationToken = default)
        {
            Call = Assert.Single(calls);
            Assert.Equal(ToolInputKind.Text, Call.InputKind);
            Assert.Equal(
                "first line\nsecond line",
                Call.Arguments);
            Result = new MultimodalToolResult(
                Call.CallId,
                [new TextContent("captured")]);
            IReadOnlyList<MultimodalToolResult> results = [Result];
            return Task.FromResult(results);
        }
    }
}
