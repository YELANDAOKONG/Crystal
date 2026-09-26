using System.Runtime.CompilerServices;
using System.Text.Json;

using Crystal;
using Crystal.Agents;
using Crystal.Chat;
using Crystal.Reasoning;
using Crystal.Tools;

namespace Crystal.Tests;

public sealed class AgentStreamingTests
{
    [Fact]
    public async Task StreamingClientEventsAreForwardedAndAssembledBeforeSelection()
    {
        var state = new OpaqueReasoningState("test", new byte[] { 1, 2, 3 });
        ChatStreamEvent[] providerEvents =
        [
            new ChatTextDelta(1, 0, ChatRole.Assistant, "other"),
            new ChatReasoningTextDelta(0, 0, 0, ReasoningTextKind.Summary, "rea"),
            new ChatReasoningTextDelta(0, 0, 0, ReasoningTextKind.Summary, "son"),
            new ChatReasoningStateReceived(0, 0, state),
            new ChatTextDelta(0, 1, ChatRole.Assistant, "answer"),
            new ChatCandidateCompleted(1, FinishReason.Stop),
            new ChatCandidateCompleted(0, FinishReason.Stop),
            new ChatUsageReceived(new TokenUsage(4, 5, 2))
        ];

        var client = new RecordingStreamingClient(providerEvents);
        ChatResponse? selectedResponse = null;
        var agent = new Agent(
            client,
            (response, _) =>
            {
                selectedResponse = response;
                return ValueTask.FromResult(0);
            });

        var initial = new ChatMessage(ChatRole.User, "question");
        var request = new AgentRunRequest(
            Guid.NewGuid(),
            [initial],
            new AgentRunLimits(1, 0, TimeSpan.FromMinutes(1)));

        var events = new List<AgentRunEvent>();
        await foreach (var runEvent in agent.StreamAsync(request))
        {
            events.Add(runEvent);
        }

        var forwarded = events.OfType<AgentModelStreamEvent>().ToArray();
        Assert.Equal(providerEvents.Length, forwarded.Length);
        for (var index = 0; index < providerEvents.Length; index++)
        {
            Assert.Same(providerEvents[index], forwarded[index].StreamEvent);
            Assert.Equal(1, forwarded[index].ModelCallNumber);
        }

        Assert.Equal(0L, events[0].Sequence);
        Assert.Equal(
            Enumerable.Range(0, events.Count).Select(static index => (long)index),
            events.Select(static runEvent => runEvent.Sequence));
        Assert.Same(initial, client.Request!.Items[0]);
        Assert.Same(selectedResponse, Assert.Single(events.OfType<AgentModelResponseEvent>()).Response);

        var response = Assert.IsType<ChatResponse>(selectedResponse);
        Assert.Equal(2, response.Candidates.Count);
        Assert.Equal("other", Assert.IsType<ChatMessage>(response.Candidates[1].Items[0]).Text);
        var reasoning = Assert.IsType<ChatReasoningItem>(response.Candidates[0].Items[0]);
        Assert.Equal("reason", Assert.Single(reasoning.Content.TextSegments).Text);
        Assert.Same(state, reasoning.Content.State);

        var result = Assert.Single(events.OfType<AgentRunCompletedEvent>()).Result;
        Assert.Equal(AgentRunStopReason.Completed, result.StopReason);
        Assert.Equal(1, result.ModelCallCount);
        Assert.Equal(3, result.Transcript.Count);
        Assert.Same(initial, result.Transcript[0]);
        Assert.Same(response.Candidates[0].Items[0], result.Transcript[1]);
        Assert.Same(response.Candidates[0].Items[1], result.Transcript[2]);
        Assert.Equal(4, result.Usage!.InputTokenCount);
        Assert.Equal(5, result.Usage.OutputTokenCount);
        Assert.False(client.CompleteCalled);
    }

    [Fact]
    public async Task IncompleteStreamFailsWithoutAddingModelContent()
    {
        ChatStreamEvent[] providerEvents =
        [
            new ChatTextDelta(0, 0, ChatRole.Assistant, "partial")
        ];
        var client = new RecordingStreamingClient(providerEvents);
        var agent = new Agent(client, (_, _) => ValueTask.FromResult(0));
        var request = new AgentRunRequest(
            Guid.NewGuid(),
            [new ChatMessage(ChatRole.User, "question")],
            new AgentRunLimits(1, 0, TimeSpan.FromMinutes(1)));

        var failure = await Assert.ThrowsAsync<InvalidOperationException>(
            () => agent.RunAsync(request));

        Assert.Contains("before a candidate completed", failure.Message);
        Assert.DoesNotContain("partial", failure.Message);
    }

    [Fact]
    public async Task ToolArgumentsAndResultsAreReplayedExactlyAcrossStreamedTurns()
    {
        var client = new ToolCallingStreamingClient();
        var executor = new RecordingToolExecutor();
        var agent = new Agent(
            client,
            (_, _) => ValueTask.FromResult(0),
            executor);
        var initial = new ChatMessage(ChatRole.User, "question");
        var request = new AgentRunRequest(
            Guid.NewGuid(),
            [initial],
            new AgentRunLimits(2, 1, TimeSpan.FromMinutes(1)));

        var result = await agent.RunAsync(request);

        Assert.Equal(2, result.ModelCallCount);
        Assert.Equal(1, result.ToolCallCount);
        Assert.Equal(2, client.Requests.Count);
        Assert.Same(initial, client.Requests[0].Items[0]);
        Assert.Same(executor.Definitions[0], client.Requests[0].Tools[0]);
        var replayedCall = Assert.IsType<ToolCall>(client.Requests[1].Items[1]);
        Assert.Equal("call-1", replayedCall.CallId);
        Assert.Equal("lookup", replayedCall.Name);
        Assert.Equal("{\"q\":\"x\"}", replayedCall.Arguments);
        Assert.Same(executor.Call, replayedCall);
        Assert.Same(executor.Result, client.Requests[1].Items[2]);
        Assert.Equal("tool output", executor.Result!.Text);
        Assert.Equal("answer", Assert.IsType<ChatMessage>(result.Transcript[3]).Text);
    }

    [Fact]
    public async Task CallerCancellationStopsStreamingWithoutACompletedResult()
    {
        ChatStreamEvent[] providerEvents =
        [
            new ChatTextDelta(0, 0, ChatRole.Assistant, "partial"),
            new ChatCandidateCompleted(0, FinishReason.Stop)
        ];
        var client = new RecordingStreamingClient(providerEvents);
        var agent = new Agent(client, (_, _) => ValueTask.FromResult(0));
        var request = new AgentRunRequest(
            Guid.NewGuid(),
            [new ChatMessage(ChatRole.User, "question")],
            new AgentRunLimits(1, 0, TimeSpan.FromMinutes(1)));
        using var cancellation = new CancellationTokenSource();
        var events = new List<AgentRunEvent>();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var runEvent in agent.StreamAsync(request, cancellation.Token))
            {
                events.Add(runEvent);
                if (runEvent is AgentModelStreamEvent)
                {
                    cancellation.Cancel();
                }
            }
        });

        Assert.Single(events.OfType<AgentModelStreamEvent>());
        Assert.Empty(events.OfType<AgentRunCompletedEvent>());
    }

    private sealed class RecordingStreamingClient(
        IReadOnlyList<ChatStreamEvent> events) : IStreamingChatClient
    {
        public ChatRequest? Request { get; private set; }

        public bool CompleteCalled { get; private set; }

        public Task<ChatResponse> CompleteAsync(
            ChatRequest request,
            CancellationToken cancellationToken = default)
        {
            CompleteCalled = true;
            throw new InvalidOperationException("The non-streaming path was called.");
        }

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

    private sealed class ToolCallingStreamingClient : IStreamingChatClient
    {
        public List<ChatRequest> Requests { get; } = [];

        public Task<ChatResponse> CompleteAsync(
            ChatRequest request,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("The non-streaming path was called.");

        public async IAsyncEnumerable<ChatStreamEvent> StreamAsync(
            ChatRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            if (Requests.Count == 1)
            {
                yield return new ChatToolCallDelta(0, 0, "call", "look", "{\"q\":");
                yield return new ChatToolCallDelta(0, 0, "-1", "up", "\"x\"}");
                yield return new ChatCandidateCompleted(0, FinishReason.ToolCalls);
            }
            else
            {
                yield return new ChatTextDelta(0, 0, ChatRole.Assistant, "answer");
                yield return new ChatCandidateCompleted(0, FinishReason.Stop);
            }

            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    private sealed class RecordingToolExecutor : IToolExecutor
    {
        public RecordingToolExecutor()
        {
            using var schema = JsonDocument.Parse("{}");
            Definitions = [new ToolDefinition("lookup", schema.RootElement)];
        }

        public IReadOnlyList<ToolDefinition> Definitions { get; }

        public ToolCall? Call { get; private set; }

        public ToolResult? Result { get; private set; }

        public Task<IReadOnlyList<ToolResult>> ExecuteAsync(
            IEnumerable<ToolCall> calls,
            CancellationToken cancellationToken = default)
        {
            Call = Assert.Single(calls);
            Assert.Equal("{\"q\":\"x\"}", Call.Arguments);
            Result = new ToolResult(Call.CallId, "tool output");
            IReadOnlyList<ToolResult> results = [Result];
            return Task.FromResult(results);
        }
    }
}
