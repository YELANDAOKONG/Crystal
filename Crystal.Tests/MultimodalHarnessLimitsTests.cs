using Crystal;
using Crystal.Multimodal;
using Crystal.Multimodal.Agents;
using Crystal.Multimodal.Chat;
using Crystal.Multimodal.Harness;

namespace Crystal.Tests;

public sealed class MultimodalHarnessLimitsTests
{
    [Fact]
    public async Task UnlimitedSessionAllowsNestedMultimodalInvocations()
    {
        var client = new RecordingMultimodalClient();
        var name = new MultimodalAgentName("worker");
        var agent = new MultimodalAgent(client, (_, _) => ValueTask.FromResult(0));
        var harness = new MultimodalAgentHarness(
            [new MultimodalAgentRegistration(name, agent)]);
        var session = harness.CreateSession(
            Guid.NewGuid(),
            MultimodalHarnessLimits.Unlimited);
        Guid? parentId = null;

        for (var index = 0; index < 3; index++)
        {
            var invocationId = Guid.NewGuid();
            var request = new MultimodalAgentInvocationRequest(
                invocationId,
                name,
                [],
                MultimodalAgentRunLimits.Unlimited,
                parentId);
            var events = new List<MultimodalHarnessEvent>();
            await foreach (var harnessEvent in session.StreamAsync(request))
            {
                events.Add(harnessEvent);
            }

            var started = Assert.Single(
                events.OfType<MultimodalHarnessInvocationStartedEvent>());
            Assert.Null(started.EffectiveLimits.MaximumModelCalls);
            Assert.Null(started.EffectiveLimits.MaximumToolCalls);
            Assert.Null(started.EffectiveLimits.MaximumDuration);
            Assert.Equal(
                MultimodalAgentInvocationOutcome.Completed,
                Assert.Single(events.OfType<MultimodalHarnessInvocationCompletedEvent>())
                    .Result.Outcome);
            parentId = invocationId;
        }

        Assert.Equal(3, client.Requests.Count);
    }

    [Fact]
    public async Task FiniteMultimodalSessionNarrowsUnlimitedRequest()
    {
        var client = new RecordingMultimodalClient();
        var name = new MultimodalAgentName("worker");
        var agent = new MultimodalAgent(client, (_, _) => ValueTask.FromResult(0));
        var harness = new MultimodalAgentHarness(
            [new MultimodalAgentRegistration(name, agent)]);
        var session = harness.CreateSession(
            Guid.NewGuid(),
            new MultimodalHarnessLimits(0, 1, 0, TimeSpan.FromMinutes(1)));
        var request = new MultimodalAgentInvocationRequest(
            Guid.NewGuid(), name, [], MultimodalAgentRunLimits.Unlimited);
        var events = new List<MultimodalHarnessEvent>();
        await foreach (var harnessEvent in session.StreamAsync(request))
        {
            events.Add(harnessEvent);
        }

        var started = Assert.Single(
            events.OfType<MultimodalHarnessInvocationStartedEvent>());
        Assert.Equal(1, started.EffectiveLimits.MaximumModelCalls);
        Assert.Equal(0, started.EffectiveLimits.MaximumToolCalls);
        Assert.NotNull(started.EffectiveLimits.MaximumDuration);
        var denied = await session.InvokeAsync(new MultimodalAgentInvocationRequest(
            Guid.NewGuid(), name, [], MultimodalAgentRunLimits.Unlimited));
        Assert.Equal(
            MultimodalAgentInvocationOutcome.ModelCallLimitReached,
            denied.Outcome);
        Assert.Single(client.Requests);
    }

    private sealed class RecordingMultimodalClient : IMultimodalChatClient
    {
        private static readonly MultimodalContentCapability Text =
            new(ContentModality.Text);

        public MultimodalChatCapabilities Capabilities { get; } =
            new([Text], [Text]);

        public List<MultimodalChatRequest> Requests { get; } = [];

        public Task<MultimodalChatResponse> CompleteAsync(
            MultimodalChatRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var response = new MultimodalChatResponse(
                [new MultimodalChatCandidate([], FinishReason.Stop)]);
            return Task.FromResult(response);
        }
    }
}
