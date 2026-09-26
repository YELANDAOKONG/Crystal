using System.Runtime.CompilerServices;

using Crystal;
using Crystal.Agents;
using Crystal.Chat;
using Crystal.Media;
using Crystal.Multimodal;
using Crystal.Multimodal.Agents;
using Crystal.Multimodal.Chat;

namespace Crystal.Tests;

public sealed class AgentStreamEquivalenceTests
{
    [Fact]
    public async Task TextStreamingAndCompleteResponsesProduceEquivalentRuns()
    {
        var usage = new TokenUsage(2, 3);
        var response = new ChatResponse(
            [new ChatCandidate(
                [new ChatMessage(ChatRole.Assistant, "answer")],
                FinishReason.Stop)],
            usage);
        ChatStreamEvent[] streamEvents =
        [
            new ChatTextDelta(0, 0, ChatRole.Assistant, "ans"),
            new ChatTextDelta(0, 0, ChatRole.Assistant, "wer"),
            new ChatCandidateCompleted(0, FinishReason.Stop),
            new ChatUsageReceived(usage)
        ];
        var completeClient = new CompleteTextClient(response);
        var streamingClient = new StreamingTextClient(streamEvents);
        var initial = new ChatMessage(ChatRole.User, "question");
        var request = new AgentRunRequest(
            Guid.NewGuid(), [initial], AgentRunLimits.Unlimited);
        var completeAgent = new Agent(completeClient, (_, _) => ValueTask.FromResult(0));
        var streamingAgent = new Agent(streamingClient, (_, _) => ValueTask.FromResult(0));

        var completeResult = await completeAgent.RunAsync(request);
        var streamingResult = await streamingAgent.RunAsync(request);

        Assert.Same(initial, Assert.Single(completeClient.Request!.Items));
        Assert.Same(initial, Assert.Single(streamingClient.Request!.Items));
        Assert.Equal(completeResult.StopReason, streamingResult.StopReason);
        Assert.Equal(completeResult.FinalFinishReason, streamingResult.FinalFinishReason);
        Assert.Equal(completeResult.ModelCallCount, streamingResult.ModelCallCount);
        Assert.Equal(completeResult.ToolCallCount, streamingResult.ToolCallCount);
        Assert.Equal(completeResult.Usage, streamingResult.Usage);
        Assert.Same(initial, completeResult.Transcript[0]);
        Assert.Same(initial, streamingResult.Transcript[0]);
        Assert.Equal(
            Assert.IsType<ChatMessage>(completeResult.Transcript[1]).Text,
            Assert.IsType<ChatMessage>(streamingResult.Transcript[1]).Text);
    }

    [Fact]
    public async Task MultimodalStreamingAndCompleteResponsesPreserveTheSameMedia()
    {
        var image = new ImageContent(new ImageMedia(
            new InlineMediaSource(new byte[] { 1, 2 }),
            new MediaMimeType("image/png")));
        var usage = new TokenUsage(2, 3);
        var response = new MultimodalChatResponse(
            [new MultimodalChatCandidate(
                [new MultimodalMessage(
                    MultimodalChatRole.Assistant,
                    [new TextContent("answer"), image])],
                FinishReason.Stop)],
            usage);
        MultimodalChatStreamEvent[] streamEvents =
        [
            new MultimodalMessageStarted(0, 0, MultimodalChatRole.Assistant),
            new MultimodalMessageTextDelta(0, 0, 0, "ans"),
            new MultimodalMessageTextDelta(0, 0, 0, "wer"),
            new MultimodalMessageContentReceived(0, 0, 1, image),
            new MultimodalChatCandidateCompleted(0, FinishReason.Stop),
            new MultimodalChatUsageReceived(usage)
        ];
        var completeClient = new CompleteMultimodalClient(response);
        var streamingClient = new StreamingMultimodalClient(streamEvents);
        var initial = new MultimodalMessage(
            MultimodalChatRole.User, [new TextContent("question")]);
        var request = new MultimodalAgentRunRequest(
            Guid.NewGuid(), [initial], MultimodalAgentRunLimits.Unlimited);
        var completeAgent = new MultimodalAgent(
            completeClient, (_, _) => ValueTask.FromResult(0));
        var streamingAgent = new MultimodalAgent(
            streamingClient, (_, _) => ValueTask.FromResult(0));

        var completeResult = await completeAgent.RunAsync(request);
        var streamingResult = await streamingAgent.RunAsync(request);

        Assert.Same(initial, Assert.Single(completeClient.Request!.Items));
        Assert.Same(initial, Assert.Single(streamingClient.Request!.Items));
        Assert.Equal(completeResult.StopReason, streamingResult.StopReason);
        Assert.Equal(completeResult.FinalFinishReason, streamingResult.FinalFinishReason);
        Assert.Equal(completeResult.ModelCallCount, streamingResult.ModelCallCount);
        Assert.Equal(completeResult.ToolCallCount, streamingResult.ToolCallCount);
        Assert.Equal(completeResult.Usage, streamingResult.Usage);
        Assert.Same(initial, completeResult.Transcript[0]);
        Assert.Same(initial, streamingResult.Transcript[0]);
        var completeMessage = Assert.IsType<MultimodalMessage>(completeResult.Transcript[1]);
        var streamedMessage = Assert.IsType<MultimodalMessage>(streamingResult.Transcript[1]);
        Assert.Equal(
            Assert.IsType<TextContent>(completeMessage.Contents[0]).Text,
            Assert.IsType<TextContent>(streamedMessage.Contents[0]).Text);
        Assert.Same(image, completeMessage.Contents[1]);
        Assert.Same(image, streamedMessage.Contents[1]);
    }

    private sealed class CompleteTextClient(ChatResponse response) : IChatClient
    {
        public ChatRequest? Request { get; private set; }

        public Task<ChatResponse> CompleteAsync(
            ChatRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Request = request;
            return Task.FromResult(response);
        }
    }

    private sealed class StreamingTextClient(IReadOnlyList<ChatStreamEvent> events)
        : IStreamingChatClient
    {
        public ChatRequest? Request { get; private set; }

        public Task<ChatResponse> CompleteAsync(
            ChatRequest request,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("The complete path was called.");

        public async IAsyncEnumerable<ChatStreamEvent> StreamAsync(
            ChatRequest request,
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

    private sealed class CompleteMultimodalClient(MultimodalChatResponse response)
        : IMultimodalChatClient
    {
        public MultimodalChatCapabilities Capabilities { get; } = CreateCapabilities();

        public MultimodalChatRequest? Request { get; private set; }

        public Task<MultimodalChatResponse> CompleteAsync(
            MultimodalChatRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Request = request;
            return Task.FromResult(response);
        }
    }

    private sealed class StreamingMultimodalClient(
        IReadOnlyList<MultimodalChatStreamEvent> events)
        : IStreamingMultimodalChatClient
    {
        public MultimodalChatCapabilities Capabilities { get; } = CreateCapabilities();

        public MultimodalChatRequest? Request { get; private set; }

        public Task<MultimodalChatResponse> CompleteAsync(
            MultimodalChatRequest request,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("The complete path was called.");

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

    private static MultimodalChatCapabilities CreateCapabilities()
    {
        var text = new MultimodalContentCapability(ContentModality.Text);
        var image = new MultimodalContentCapability(
            ContentModality.Image, [MediaSourceKind.Inline]);
        return new MultimodalChatCapabilities([text], [text, image]);
    }
}
