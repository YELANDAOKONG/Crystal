using System.Runtime.CompilerServices;

using Crystal;
using Crystal.Agents;
using Crystal.Chat;
using Crystal.Multimodal;
using Crystal.Multimodal.Agents;
using Crystal.Multimodal.Chat;

namespace Crystal.Tests;

public sealed class AgentDurationBoundaryTests
{
    private static readonly TimeSpan Duration = TimeSpan.FromMilliseconds(10);
    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(5);

    [Fact]
    public async Task TextClientReturningAfterDurationCannotCompleteRun()
    {
        var client = new LateTextClient();
        var agent = new Agent(client, (_, _) => ValueTask.FromResult(0));
        var result = await agent.RunAsync(CreateTextRequest()).WaitAsync(TestTimeout);

        Assert.Equal(AgentRunStopReason.DurationLimitReached, result.StopReason);
        Assert.Equal(1, result.ModelCallCount);
        Assert.True(client.CancellationObserved);
    }

    [Fact]
    public async Task MultimodalClientReturningAfterDurationCannotCompleteRun()
    {
        var client = new LateMultimodalClient();
        var agent = new MultimodalAgent(client, (_, _) => ValueTask.FromResult(0));
        var result = await agent.RunAsync(CreateMultimodalRequest()).WaitAsync(TestTimeout);

        Assert.Equal(MultimodalAgentRunStopReason.DurationLimitReached, result.StopReason);
        Assert.Equal(1, result.ModelCallCount);
        Assert.True(client.CancellationObserved);
    }

    [Fact]
    public async Task TextStreamEventReturnedAfterDurationIsNotForwarded()
    {
        var client = new LateTextStreamingClient();
        var agent = new Agent(client, (_, _) => ValueTask.FromResult(0));
        var events = new List<AgentRunEvent>();

        await foreach (var runEvent in agent.StreamAsync(CreateTextRequest()))
        {
            events.Add(runEvent);
        }

        Assert.True(client.CancellationObserved);
        Assert.Empty(events.OfType<AgentModelStreamEvent>());
        Assert.Equal(
            AgentRunStopReason.DurationLimitReached,
            Assert.Single(events.OfType<AgentRunCompletedEvent>()).Result.StopReason);
    }

    [Fact]
    public async Task MultimodalStreamEventReturnedAfterDurationIsNotForwarded()
    {
        var client = new LateMultimodalStreamingClient();
        var agent = new MultimodalAgent(client, (_, _) => ValueTask.FromResult(0));
        var events = new List<MultimodalAgentRunEvent>();

        await foreach (var runEvent in agent.StreamAsync(CreateMultimodalRequest()))
        {
            events.Add(runEvent);
        }

        Assert.True(client.CancellationObserved);
        Assert.Empty(events.OfType<MultimodalAgentModelStreamEvent>());
        Assert.Equal(
            MultimodalAgentRunStopReason.DurationLimitReached,
            Assert.Single(events.OfType<MultimodalAgentRunCompletedEvent>()).Result.StopReason);
    }

    [Fact]
    public async Task TextModelRequestPausedPastDurationDoesNotStartClient()
    {
        var client = new ImmediateTextClient();
        var agent = new Agent(client, (_, _) => ValueTask.FromResult(0));
        await using var enumerator = agent.StreamAsync(CreateTextRequest()).GetAsyncEnumerator();

        Assert.True(await enumerator.MoveNextAsync());
        Assert.IsType<AgentModelRequestEvent>(enumerator.Current);

        await Task.Delay(TimeSpan.FromMilliseconds(100));

        Assert.True(await enumerator.MoveNextAsync());
        var completed = Assert.IsType<AgentRunCompletedEvent>(enumerator.Current);
        Assert.Equal(AgentRunStopReason.DurationLimitReached, completed.Result.StopReason);
        Assert.Equal(0, client.CallCount);
        Assert.False(await enumerator.MoveNextAsync());
    }

    [Fact]
    public async Task MultimodalModelRequestPausedPastDurationDoesNotStartClient()
    {
        var client = new ImmediateMultimodalClient();
        var agent = new MultimodalAgent(client, (_, _) => ValueTask.FromResult(0));
        await using var enumerator = agent.StreamAsync(CreateMultimodalRequest()).GetAsyncEnumerator();

        Assert.True(await enumerator.MoveNextAsync());
        Assert.IsType<MultimodalAgentModelRequestEvent>(enumerator.Current);

        await Task.Delay(TimeSpan.FromMilliseconds(100));

        Assert.True(await enumerator.MoveNextAsync());
        var completed = Assert.IsType<MultimodalAgentRunCompletedEvent>(enumerator.Current);
        Assert.Equal(MultimodalAgentRunStopReason.DurationLimitReached, completed.Result.StopReason);
        Assert.Equal(0, client.CallCount);
        Assert.False(await enumerator.MoveNextAsync());
    }

    [Fact]
    public async Task TextCandidateSelectionPausedPastDurationDoesNotCompleteNormally()
    {
        var client = new ImmediateTextClient();
        var agent = new Agent(client, (_, _) => ValueTask.FromResult(0));
        await using var enumerator = agent.StreamAsync(CreateTextRequest()).GetAsyncEnumerator();

        while (await enumerator.MoveNextAsync())
        {
            if (enumerator.Current is AgentCandidateSelectedEvent)
            {
                break;
            }
        }

        Assert.IsType<AgentCandidateSelectedEvent>(enumerator.Current);
        await Task.Delay(TimeSpan.FromMilliseconds(100));

        Assert.True(await enumerator.MoveNextAsync());
        var completed = Assert.IsType<AgentRunCompletedEvent>(enumerator.Current);
        Assert.Equal(AgentRunStopReason.DurationLimitReached, completed.Result.StopReason);
        Assert.Equal(1, client.CallCount);
        Assert.False(await enumerator.MoveNextAsync());
    }

    [Fact]
    public async Task MultimodalCandidateSelectionPausedPastDurationDoesNotCompleteNormally()
    {
        var client = new ImmediateMultimodalClient();
        var agent = new MultimodalAgent(client, (_, _) => ValueTask.FromResult(0));
        await using var enumerator = agent.StreamAsync(CreateMultimodalRequest()).GetAsyncEnumerator();

        while (await enumerator.MoveNextAsync())
        {
            if (enumerator.Current is MultimodalAgentCandidateSelectedEvent)
            {
                break;
            }
        }

        Assert.IsType<MultimodalAgentCandidateSelectedEvent>(enumerator.Current);
        await Task.Delay(TimeSpan.FromMilliseconds(100));

        Assert.True(await enumerator.MoveNextAsync());
        var completed = Assert.IsType<MultimodalAgentRunCompletedEvent>(enumerator.Current);
        Assert.Equal(MultimodalAgentRunStopReason.DurationLimitReached, completed.Result.StopReason);
        Assert.Equal(1, client.CallCount);
        Assert.False(await enumerator.MoveNextAsync());
    }

    private static AgentRunRequest CreateTextRequest() =>
        new(Guid.NewGuid(), [], new AgentRunLimits(null, null, Duration));

    private static MultimodalAgentRunRequest CreateMultimodalRequest() =>
        new(Guid.NewGuid(), [], new MultimodalAgentRunLimits(null, null, Duration));

    private static async Task WaitForCancellationAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private sealed class LateTextClient : IChatClient
    {
        public bool CancellationObserved { get; private set; }

        public async Task<ChatResponse> CompleteAsync(
            ChatRequest request,
            CancellationToken cancellationToken = default)
        {
            await WaitForCancellationAsync(cancellationToken);
            CancellationObserved = cancellationToken.IsCancellationRequested;
            return new ChatResponse([new ChatCandidate([], FinishReason.Stop)]);
        }
    }

    private sealed class ImmediateTextClient : IChatClient
    {
        public int CallCount { get; private set; }

        public Task<ChatResponse> CompleteAsync(
            ChatRequest request,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(
                new ChatResponse([new ChatCandidate([], FinishReason.Stop)]));
        }
    }

    private sealed class ImmediateMultimodalClient : IMultimodalChatClient
    {
        private static readonly MultimodalContentCapability Text =
            new(ContentModality.Text);

        public MultimodalChatCapabilities Capabilities { get; } =
            new([Text], [Text]);

        public int CallCount { get; private set; }

        public Task<MultimodalChatResponse> CompleteAsync(
            MultimodalChatRequest request,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(
                new MultimodalChatResponse(
                    [new MultimodalChatCandidate([], FinishReason.Stop)]));
        }
    }

    private sealed class LateMultimodalClient : IMultimodalChatClient
    {
        private static readonly MultimodalContentCapability Text =
            new(ContentModality.Text);

        public MultimodalChatCapabilities Capabilities { get; } =
            new([Text], [Text]);

        public bool CancellationObserved { get; private set; }

        public async Task<MultimodalChatResponse> CompleteAsync(
            MultimodalChatRequest request,
            CancellationToken cancellationToken = default)
        {
            await WaitForCancellationAsync(cancellationToken);
            CancellationObserved = cancellationToken.IsCancellationRequested;
            return new MultimodalChatResponse(
                [new MultimodalChatCandidate([], FinishReason.Stop)]);
        }
    }

    private sealed class LateTextStreamingClient : IStreamingChatClient
    {
        public bool CancellationObserved { get; private set; }

        public Task<ChatResponse> CompleteAsync(
            ChatRequest request,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("The complete path was called.");

        public async IAsyncEnumerable<ChatStreamEvent> StreamAsync(
            ChatRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await WaitForCancellationAsync(cancellationToken);
            CancellationObserved = cancellationToken.IsCancellationRequested;
            yield return new ChatTextDelta(0, 0, ChatRole.Assistant, "late");
            yield return new ChatCandidateCompleted(0, FinishReason.Stop);
        }
    }

    private sealed class LateMultimodalStreamingClient
        : IStreamingMultimodalChatClient
    {
        private static readonly MultimodalContentCapability Text =
            new(ContentModality.Text);

        public MultimodalChatCapabilities Capabilities { get; } =
            new([Text], [Text]);

        public bool CancellationObserved { get; private set; }

        public Task<MultimodalChatResponse> CompleteAsync(
            MultimodalChatRequest request,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("The complete path was called.");

        public async IAsyncEnumerable<MultimodalChatStreamEvent> StreamAsync(
            MultimodalChatRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await WaitForCancellationAsync(cancellationToken);
            CancellationObserved = cancellationToken.IsCancellationRequested;
            yield return new MultimodalMessageStarted(
                0, 0, MultimodalChatRole.Assistant);
            yield return new MultimodalChatCandidateCompleted(0, FinishReason.Stop);
        }
    }
}
