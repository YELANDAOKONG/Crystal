using Crystal;
using Crystal.Agents;
using Crystal.Chat;
using Crystal.Harness;

namespace Crystal.Tests;

public sealed class HarnessSessionTests
{
    [Fact]
    public async Task ExplicitChildInvocationsShareBudgetAndPreserveEventAncestry()
    {
        var response = new ChatResponse(
            [new ChatCandidate([new ChatMessage(ChatRole.Assistant, "answer")], FinishReason.Stop)]);
        var client = new RecordingChatClient(response);
        var agent = new Agent(client, (_, _) => ValueTask.FromResult(0));
        var name = new AgentName("worker");
        var harness = new AgentHarness([new AgentRegistration(name, agent)]);
        var sessionId = Guid.NewGuid();
        var session = harness.CreateSession(
            sessionId,
            new HarnessLimits(1, 2, 0, TimeSpan.FromMinutes(1)));
        var requestedLimits = new AgentRunLimits(2, 0, TimeSpan.FromMinutes(1));
        var initial = new ChatMessage(ChatRole.User, "question");
        var rootId = Guid.NewGuid();
        var rootRequest = new AgentInvocationRequest(
            rootId, name, [initial], requestedLimits);
        var rootEvents = new List<HarnessEvent>();

        await foreach (var harnessEvent in session.StreamAsync(rootRequest))
        {
            rootEvents.Add(harnessEvent);
        }

        var rootStarted = Assert.Single(rootEvents.OfType<HarnessInvocationStartedEvent>());
        Assert.Equal(2, rootStarted.EffectiveLimits.MaximumModelCalls);
        Assert.Equal(0, rootStarted.Sequence);
        Assert.Equal(
            Enumerable.Range(0, rootEvents.Count).Select(static index => (long)index),
            rootEvents.Select(static harnessEvent => harnessEvent.Sequence));
        Assert.All(rootEvents, harnessEvent =>
        {
            Assert.Equal(sessionId, harnessEvent.SessionId);
            Assert.Equal(rootId, harnessEvent.InvocationId);
            Assert.Null(harnessEvent.ParentInvocationId);
        });
        var forwardedResponse = Assert.Single(rootEvents
            .OfType<HarnessAgentEvent>()
            .Select(static harnessEvent => harnessEvent.AgentEvent)
            .OfType<AgentModelResponseEvent>());
        Assert.Same(response, forwardedResponse.Response);
        Assert.Same(initial, Assert.Single(client.Requests[0].Items));
        var rootResult = Assert.Single(
            rootEvents.OfType<HarnessInvocationCompletedEvent>()).Result;
        Assert.Equal(AgentInvocationOutcome.Completed, rootResult.Outcome);
        Assert.Equal(1, rootResult.AgentResult!.ModelCallCount);

        var childId = Guid.NewGuid();
        var childRequest = new AgentInvocationRequest(
            childId, name, [initial], requestedLimits, rootId);
        var childEvents = new List<HarnessEvent>();
        await foreach (var harnessEvent in session.StreamAsync(childRequest))
        {
            childEvents.Add(harnessEvent);
        }

        var childStarted = Assert.Single(childEvents.OfType<HarnessInvocationStartedEvent>());
        Assert.Equal(1, childStarted.EffectiveLimits.MaximumModelCalls);
        Assert.All(childEvents, harnessEvent =>
        {
            Assert.Equal(sessionId, harnessEvent.SessionId);
            Assert.Equal(childId, harnessEvent.InvocationId);
            Assert.Equal(rootId, harnessEvent.ParentInvocationId);
        });
        Assert.Equal(
            AgentInvocationOutcome.Completed,
            Assert.Single(childEvents.OfType<HarnessInvocationCompletedEvent>())
                .Result.Outcome);

        var sibling = await session.InvokeAsync(new AgentInvocationRequest(
            Guid.NewGuid(), name, [initial], requestedLimits));
        Assert.Equal(AgentInvocationOutcome.ModelCallLimitReached, sibling.Outcome);
        Assert.Null(sibling.AgentResult);
        var grandchild = await session.InvokeAsync(new AgentInvocationRequest(
            Guid.NewGuid(), name, [initial], requestedLimits, childId));
        Assert.Equal(AgentInvocationOutcome.DepthLimitReached, grandchild.Outcome);
        Assert.Null(grandchild.AgentResult);
        Assert.Equal(2, client.Requests.Count);
    }

    private sealed class RecordingChatClient(ChatResponse response) : IChatClient
    {
        public List<ChatRequest> Requests { get; } = [];

        public Task<ChatResponse> CompleteAsync(
            ChatRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(response);
        }
    }
}
