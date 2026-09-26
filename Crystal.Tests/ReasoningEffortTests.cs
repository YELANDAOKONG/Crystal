using Crystal;
using Crystal.Agents;
using Crystal.Chat;
using Crystal.Completions;
using Crystal.Generation.Audio;
using Crystal.Generation.Images;
using Crystal.Generation.Video;
using Crystal.Harness;
using Crystal.Multimodal;
using Crystal.Multimodal.Agents;
using Crystal.Multimodal.Chat;
using Crystal.Multimodal.Harness;
using Crystal.Reasoning;

namespace Crystal.Tests;

public sealed class ReasoningEffortTests
{
    [Fact]
    public void CallerDefinedEffortRemainsAvailableAcrossRequestFamilies()
    {
        var effort = new ReasoningEffort("very-high");
        var reasoning = new ReasoningOptions(effort: effort);

        Assert.Equal("very-high", effort.Value);
        Assert.NotEqual(ReasoningEffort.Maximum, effort);
        Assert.Same(reasoning, new CompletionRequest("prompt", reasoning).Reasoning);
        Assert.Same(reasoning, new ChatRequest([], reasoning: reasoning).Reasoning);
        Assert.Same(reasoning, new MultimodalChatRequest([], reasoning: reasoning).Reasoning);
        Assert.Same(reasoning, new ImageGenerationRequest([], reasoning: reasoning).Reasoning);
        Assert.Same(reasoning, new AudioGenerationRequest([], reasoning: reasoning).Reasoning);
        Assert.Same(reasoning, new VideoGenerationRequest([], reasoning: reasoning).Reasoning);
    }

    [Fact]
    public async Task TextHarnessForwardsCallerDefinedEffortToClient()
    {
        var effort = new ReasoningEffort("very-high");
        var reasoning = new ReasoningOptions(effort: effort);
        var client = new RecordingTextClient();
        var name = new AgentName("worker");
        var agent = new Agent(client, (_, _) => ValueTask.FromResult(0));
        var harness = new AgentHarness([new AgentRegistration(name, agent)]);
        var session = harness.CreateSession(Guid.NewGuid(), HarnessLimits.Unlimited);
        var request = new AgentInvocationRequest(
            Guid.NewGuid(),
            name,
            [new ChatMessage(ChatRole.User, "question")],
            AgentRunLimits.Unlimited,
            reasoning: reasoning);

        var result = await session.InvokeAsync(request);

        Assert.Equal(AgentInvocationOutcome.Completed, result.Outcome);
        Assert.Same(reasoning, client.Request!.Reasoning);
        Assert.Same(effort, client.Request.Reasoning!.Effort);
        Assert.Equal("very-high", client.Request.Reasoning.Effort!.Value);
    }

    [Fact]
    public async Task MultimodalHarnessForwardsCallerDefinedEffortToClient()
    {
        var effort = new ReasoningEffort("very-high");
        var reasoning = new ReasoningOptions(effort: effort);
        var client = new RecordingMultimodalClient();
        var name = new MultimodalAgentName("worker");
        var agent = new MultimodalAgent(client, (_, _) => ValueTask.FromResult(0));
        var harness = new MultimodalAgentHarness(
            [new MultimodalAgentRegistration(name, agent)]);
        var session = harness.CreateSession(
            Guid.NewGuid(), MultimodalHarnessLimits.Unlimited);
        var request = new MultimodalAgentInvocationRequest(
            Guid.NewGuid(),
            name,
            [new MultimodalMessage(
                MultimodalChatRole.User,
                [new TextContent("question")])],
            MultimodalAgentRunLimits.Unlimited,
            reasoning: reasoning);

        var result = await session.InvokeAsync(request);

        Assert.Equal(MultimodalAgentInvocationOutcome.Completed, result.Outcome);
        Assert.Same(reasoning, client.Request!.Reasoning);
        Assert.Same(effort, client.Request.Reasoning!.Effort);
        Assert.Equal("very-high", client.Request.Reasoning.Effort!.Value);
    }

    private sealed class RecordingTextClient : IChatClient
    {
        public ChatRequest? Request { get; private set; }

        public Task<ChatResponse> CompleteAsync(
            ChatRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Request = request;
            return Task.FromResult(new ChatResponse(
                [new ChatCandidate([], FinishReason.Stop)]));
        }
    }

    private sealed class RecordingMultimodalClient : IMultimodalChatClient
    {
        private static readonly MultimodalContentCapability Text =
            new(ContentModality.Text);

        public MultimodalChatCapabilities Capabilities { get; } =
            new([Text], [Text], supportsReasoningOptions: true);

        public MultimodalChatRequest? Request { get; private set; }

        public Task<MultimodalChatResponse> CompleteAsync(
            MultimodalChatRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Request = request;
            return Task.FromResult(new MultimodalChatResponse(
                [new MultimodalChatCandidate([], FinishReason.Stop)]));
        }
    }
}
