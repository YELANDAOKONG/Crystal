using System.Runtime.CompilerServices;

using Crystal;
using Crystal.Agents;
using Crystal.Chat;
using Crystal.Multimodal;
using Crystal.Multimodal.Agents;
using Crystal.Multimodal.Chat;

namespace Crystal.Tests;

public sealed class AgentCancellationBoundaryTests
{
    [Fact]
    public async Task PreCanceledTextRunDoesNotStartTheClient()
    {
        var client = new IgnoringTextClient();
        var agent = new Agent(client, (_, _) => ValueTask.FromResult(0));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            agent.RunAsync(CreateTextRequest(), cancellation.Token));

        Assert.Equal(0, client.CallCount);
    }

    [Fact]
    public async Task TextCancellationAfterRequestEventPreventsClientStart()
    {
        var client = new IgnoringTextClient();
        var agent = new Agent(client, (_, _) => ValueTask.FromResult(0));
        using var cancellation = new CancellationTokenSource();
        await using var events = agent.StreamAsync(
            CreateTextRequest(), cancellation.Token).GetAsyncEnumerator();

        Assert.True(await events.MoveNextAsync());
        Assert.IsType<AgentModelRequestEvent>(events.Current);
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await events.MoveNextAsync();
        });
        Assert.Equal(0, client.CallCount);
    }

    [Fact]
    public async Task TextCancellationAfterCandidateEventPreventsCompletion()
    {
        var client = new IgnoringTextClient();
        var agent = new Agent(client, (_, _) => ValueTask.FromResult(0));
        using var cancellation = new CancellationTokenSource();
        await using var events = agent.StreamAsync(
            CreateTextRequest(), cancellation.Token).GetAsyncEnumerator();

        for (var index = 0; index < 3; index++)
        {
            Assert.True(await events.MoveNextAsync());
        }

        Assert.IsType<AgentCandidateSelectedEvent>(events.Current);
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await events.MoveNextAsync();
        });
        Assert.Equal(1, client.CallCount);
    }

    [Fact]
    public async Task TextClientIgnoringCancellationCannotCompleteRun()
    {
        using var cancellation = new CancellationTokenSource();
        var client = new IgnoringTextClient(() => cancellation.Cancel());
        var agent = new Agent(client, (_, _) => ValueTask.FromResult(0));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            agent.RunAsync(CreateTextRequest(), cancellation.Token));

        Assert.Equal(1, client.CallCount);
    }

    [Fact]
    public async Task TextStreamEventReturnedAfterCancellationIsNotForwarded()
    {
        using var cancellation = new CancellationTokenSource();
        var client = new IgnoringTextStreamingClient(() => cancellation.Cancel());
        var agent = new Agent(client, (_, _) => ValueTask.FromResult(0));
        var events = new List<AgentRunEvent>();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var runEvent in agent.StreamAsync(
                CreateTextRequest(), cancellation.Token))
            {
                events.Add(runEvent);
            }
        });

        Assert.True(client.Started);
        Assert.Empty(events.OfType<AgentModelStreamEvent>());
    }

    [Fact]
    public async Task PreCanceledMultimodalRunDoesNotStartTheClient()
    {
        var client = new IgnoringMultimodalClient();
        var agent = new MultimodalAgent(client, (_, _) => ValueTask.FromResult(0));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            agent.RunAsync(CreateMultimodalRequest(), cancellation.Token));

        Assert.Equal(0, client.CallCount);
    }

    [Fact]
    public async Task MultimodalCancellationAfterRequestEventPreventsClientStart()
    {
        var client = new IgnoringMultimodalClient();
        var agent = new MultimodalAgent(client, (_, _) => ValueTask.FromResult(0));
        using var cancellation = new CancellationTokenSource();
        await using var events = agent.StreamAsync(
            CreateMultimodalRequest(), cancellation.Token).GetAsyncEnumerator();

        Assert.True(await events.MoveNextAsync());
        Assert.IsType<MultimodalAgentModelRequestEvent>(events.Current);
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await events.MoveNextAsync();
        });
        Assert.Equal(0, client.CallCount);
    }

    [Fact]
    public async Task MultimodalCancellationAfterCandidateEventPreventsCompletion()
    {
        var client = new IgnoringMultimodalClient();
        var agent = new MultimodalAgent(client, (_, _) => ValueTask.FromResult(0));
        using var cancellation = new CancellationTokenSource();
        await using var events = agent.StreamAsync(
            CreateMultimodalRequest(), cancellation.Token).GetAsyncEnumerator();

        for (var index = 0; index < 3; index++)
        {
            Assert.True(await events.MoveNextAsync());
        }

        Assert.IsType<MultimodalAgentCandidateSelectedEvent>(events.Current);
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await events.MoveNextAsync();
        });
        Assert.Equal(1, client.CallCount);
    }

    [Fact]
    public async Task MultimodalClientIgnoringCancellationCannotCompleteRun()
    {
        using var cancellation = new CancellationTokenSource();
        var client = new IgnoringMultimodalClient(() => cancellation.Cancel());
        var agent = new MultimodalAgent(client, (_, _) => ValueTask.FromResult(0));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            agent.RunAsync(CreateMultimodalRequest(), cancellation.Token));

        Assert.Equal(1, client.CallCount);
    }

    [Fact]
    public async Task MultimodalStreamEventReturnedAfterCancellationIsNotForwarded()
    {
        using var cancellation = new CancellationTokenSource();
        var client = new IgnoringMultimodalStreamingClient(() => cancellation.Cancel());
        var agent = new MultimodalAgent(client, (_, _) => ValueTask.FromResult(0));
        var events = new List<MultimodalAgentRunEvent>();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var runEvent in agent.StreamAsync(
                CreateMultimodalRequest(), cancellation.Token))
            {
                events.Add(runEvent);
            }
        });

        Assert.True(client.Started);
        Assert.Empty(events.OfType<MultimodalAgentModelStreamEvent>());
    }

    private static AgentRunRequest CreateTextRequest() =>
        new(Guid.NewGuid(), [], AgentRunLimits.Unlimited);

    private static MultimodalAgentRunRequest CreateMultimodalRequest() =>
        new(Guid.NewGuid(), [], MultimodalAgentRunLimits.Unlimited);

    private sealed class IgnoringTextClient(Action? onComplete = null) : IChatClient
    {
        public int CallCount { get; private set; }

        public Task<ChatResponse> CompleteAsync(
            ChatRequest request,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            onComplete?.Invoke();
            return Task.FromResult(new ChatResponse(
                [new ChatCandidate([], FinishReason.Stop)]));
        }
    }

    private sealed class IgnoringMultimodalClient(Action? onComplete = null)
        : IMultimodalChatClient
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
            onComplete?.Invoke();
            return Task.FromResult(new MultimodalChatResponse(
                [new MultimodalChatCandidate([], FinishReason.Stop)]));
        }
    }

    private sealed class IgnoringTextStreamingClient(Action onYield)
        : IStreamingChatClient
    {
        public bool Started { get; private set; }

        public Task<ChatResponse> CompleteAsync(
            ChatRequest request,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("The complete path was called.");

        public async IAsyncEnumerable<ChatStreamEvent> StreamAsync(
            ChatRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            Started = true;
            onYield();
            yield return new ChatTextDelta(0, 0, ChatRole.Assistant, "answer");
            await Task.Yield();
            yield return new ChatCandidateCompleted(0, FinishReason.Stop);
        }
    }

    private sealed class IgnoringMultimodalStreamingClient(Action onYield)
        : IStreamingMultimodalChatClient
    {
        private static readonly MultimodalContentCapability Text =
            new(ContentModality.Text);

        public MultimodalChatCapabilities Capabilities { get; } =
            new([Text], [Text]);

        public bool Started { get; private set; }

        public Task<MultimodalChatResponse> CompleteAsync(
            MultimodalChatRequest request,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("The complete path was called.");

        public async IAsyncEnumerable<MultimodalChatStreamEvent> StreamAsync(
            MultimodalChatRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            Started = true;
            onYield();
            yield return new MultimodalMessageStarted(
                0, 0, MultimodalChatRole.Assistant);
            await Task.Yield();
            yield return new MultimodalChatCandidateCompleted(0, FinishReason.Stop);
        }
    }
}
