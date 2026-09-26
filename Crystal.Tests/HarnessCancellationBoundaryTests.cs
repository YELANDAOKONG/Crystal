using System.Runtime.CompilerServices;

using Crystal;
using Crystal.Agents;
using Crystal.Chat;
using Crystal.Harness;
using Crystal.Multimodal;
using Crystal.Multimodal.Agents;
using Crystal.Multimodal.Chat;
using Crystal.Multimodal.Harness;

namespace Crystal.Tests;

public sealed class HarnessCancellationBoundaryTests : IAgent, IMultimodalAgent
{
    private static readonly MultimodalContentCapability _textCapability =
        new(ContentModality.Text);

    private int _textStarts;
    private int _multimodalStarts;

    public MultimodalChatCapabilities Capabilities { get; } =
        new([_textCapability], [_textCapability]);

    [Fact]
    public async Task TextCallerCancellationAfterStartPreventsAgentInvocation()
    {
        var name = new AgentName("worker");
        var harness = new AgentHarness([new AgentRegistration(name, this)]);
        var session = harness.CreateSession(
            Guid.NewGuid(), new HarnessLimits(0, 1, 0, null));
        var request = new AgentInvocationRequest(
            Guid.NewGuid(), name, [], AgentRunLimits.Unlimited);
        using var cancellation = new CancellationTokenSource();
        await using var events = session.StreamAsync(
            request, cancellation.Token).GetAsyncEnumerator();

        Assert.True(await events.MoveNextAsync());
        Assert.IsType<HarnessInvocationStartedEvent>(events.Current);
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await events.MoveNextAsync();
        });
        Assert.Equal(0, _textStarts);

        var next = await session.InvokeAsync(new AgentInvocationRequest(
            Guid.NewGuid(), name, [], AgentRunLimits.Unlimited));
        Assert.Equal(AgentInvocationOutcome.ModelCallLimitReached, next.Outcome);
    }

    [Fact]
    public async Task TextCallerCancellationAfterForwardedEventSuppressesCompletion()
    {
        var name = new AgentName("worker");
        var harness = new AgentHarness([new AgentRegistration(name, this)]);
        var session = harness.CreateSession(Guid.NewGuid(), HarnessLimits.Unlimited);
        var request = new AgentInvocationRequest(
            Guid.NewGuid(), name, [], AgentRunLimits.Unlimited);
        using var cancellation = new CancellationTokenSource();
        await using var events = session.StreamAsync(
            request, cancellation.Token).GetAsyncEnumerator();

        Assert.True(await events.MoveNextAsync());
        Assert.True(await events.MoveNextAsync());
        var forwarded = Assert.IsType<HarnessAgentEvent>(events.Current);
        Assert.IsType<AgentModelRequestEvent>(forwarded.AgentEvent);
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await events.MoveNextAsync();
        });
        Assert.Equal(1, _textStarts);
    }

    [Fact]
    public async Task MultimodalSessionCancellationAfterStartPreventsAgentInvocation()
    {
        var name = new MultimodalAgentName("worker");
        var harness = new MultimodalAgentHarness(
            [new MultimodalAgentRegistration(name, this)]);
        using var cancellation = new CancellationTokenSource();
        var session = harness.CreateSession(
            Guid.NewGuid(), MultimodalHarnessLimits.Unlimited,
            cancellation.Token);
        var request = new MultimodalAgentInvocationRequest(
            Guid.NewGuid(), name, [], MultimodalAgentRunLimits.Unlimited);
        await using var events = session.StreamAsync(request).GetAsyncEnumerator();

        Assert.True(await events.MoveNextAsync());
        Assert.IsType<MultimodalHarnessInvocationStartedEvent>(events.Current);
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await events.MoveNextAsync();
        });
        Assert.Equal(0, _multimodalStarts);
    }

    [Fact]
    public async Task MultimodalSessionCancellationAfterForwardedEventSuppressesCompletion()
    {
        var name = new MultimodalAgentName("worker");
        var harness = new MultimodalAgentHarness(
            [new MultimodalAgentRegistration(name, this)]);
        using var cancellation = new CancellationTokenSource();
        var session = harness.CreateSession(
            Guid.NewGuid(), MultimodalHarnessLimits.Unlimited,
            cancellation.Token);
        var request = new MultimodalAgentInvocationRequest(
            Guid.NewGuid(), name, [], MultimodalAgentRunLimits.Unlimited);
        await using var events = session.StreamAsync(request).GetAsyncEnumerator();

        Assert.True(await events.MoveNextAsync());
        Assert.True(await events.MoveNextAsync());
        var forwarded = Assert.IsType<MultimodalHarnessAgentEvent>(events.Current);
        Assert.IsType<MultimodalAgentModelRequestEvent>(forwarded.AgentEvent);
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await events.MoveNextAsync();
        });
        Assert.Equal(1, _multimodalStarts);
    }

    public Task<AgentRunResult> RunAsync(
        AgentRunRequest request,
        CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("The run path was called.");

    public async IAsyncEnumerable<AgentRunEvent> StreamAsync(
        AgentRunRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _textStarts++;
        yield return new AgentModelRequestEvent(
            request.RunId, 0, 1, new ChatRequest(request.Items));
        await Task.Yield();
        yield return new AgentRunCompletedEvent(
            request.RunId,
            1,
            new AgentRunResult(
                request.RunId, request.Items, AgentRunStopReason.Completed,
                1, 0, finalFinishReason: FinishReason.Stop));
    }

    public Task<MultimodalAgentRunResult> RunAsync(
        MultimodalAgentRunRequest request,
        CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("The run path was called.");

    public async IAsyncEnumerable<MultimodalAgentRunEvent> StreamAsync(
        MultimodalAgentRunRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _multimodalStarts++;
        yield return new MultimodalAgentModelRequestEvent(
            request.RunId, 0, 1, new MultimodalChatRequest(request.Items));
        await Task.Yield();
        yield return new MultimodalAgentRunCompletedEvent(
            request.RunId,
            1,
            new MultimodalAgentRunResult(
                request.RunId, request.Items,
                MultimodalAgentRunStopReason.Completed,
                1, 0, finalFinishReason: FinishReason.Stop));
    }
}
