using System.Text.Json;

using Crystal;
using Crystal.Agents;
using Crystal.Chat;
using Crystal.Completions;
using Crystal.Harness;
using Crystal.Multimodal;
using Crystal.Multimodal.Agents;
using Crystal.Multimodal.Chat;
using Crystal.Multimodal.Harness;

namespace Crystal.Tests;

public sealed class JsonOutputRequirementTests : IChatClient, IMultimodalChatClient
{
    private static readonly MultimodalContentCapability _text =
        new(ContentModality.Text);

    public MultimodalChatCapabilities Capabilities { get; } =
        new([_text], [_text]);

    public ChatRequest? LastTextRequest { get; private set; }

    public MultimodalChatRequest? LastMultimodalRequest { get; private set; }

    [Fact]
    public void RequirementCopiesSchemaAndKeepsItOutOfDiagnosticText()
    {
        JsonOutputRequirement requirement;
        using (var document = JsonDocument.Parse(
            "{\"type\":\"object\",\"description\":\"private\"}"))
        {
            requirement = new JsonOutputRequirement(document.RootElement);
        }

        Assert.Equal("object", requirement.Schema.GetProperty("type").GetString());
        Assert.Equal(nameof(JsonOutputRequirement), requirement.ToString());
        Assert.Equal(
            "schema",
            Assert.Throws<ArgumentException>(() =>
                new JsonOutputRequirement(default)).ParamName);

        using var invalid = JsonDocument.Parse("[1]");
        Assert.Equal(
            "schema",
            Assert.Throws<ArgumentException>(() =>
                new JsonOutputRequirement(invalid.RootElement)).ParamName);

        using var booleanSchema = JsonDocument.Parse("true");
        Assert.Equal(
            JsonValueKind.True,
            new JsonOutputRequirement(booleanSchema.RootElement)
                .Schema.ValueKind);

        Assert.Same(
            requirement,
            new CompletionRequest("prompt", jsonOutput: requirement).JsonOutput);
        Assert.Same(
            requirement,
            new ChatRequest([], jsonOutput: requirement).JsonOutput);
        Assert.Same(
            requirement,
            new MultimodalChatRequest([], jsonOutput: requirement).JsonOutput);
    }

    [Fact]
    public async Task TextHarnessAndAgentForwardExactRequirement()
    {
        var requirement = CreateRequirement();
        var name = new AgentName("worker");
        var agent = new Agent(this, (_, _) => ValueTask.FromResult(0));
        var harness = new AgentHarness([new AgentRegistration(name, agent)]);
        var session = harness.CreateSession(Guid.NewGuid(), HarnessLimits.Unlimited);
        var invocation = new AgentInvocationRequest(
            Guid.NewGuid(), name, [], AgentRunLimits.Unlimited,
            jsonOutput: requirement);

        var result = await session.InvokeAsync(invocation);

        Assert.Equal(AgentInvocationOutcome.Completed, result.Outcome);
        Assert.Same(requirement, LastTextRequest?.JsonOutput);
        Assert.Equal("{\"ok\":true}",
            Assert.IsType<ChatMessage>(
                Assert.Single(result.AgentResult!.Transcript)).Text);
    }

    [Fact]
    public async Task MultimodalHarnessAndAgentForwardExactRequirement()
    {
        var requirement = CreateRequirement();
        var name = new MultimodalAgentName("worker");
        var agent = new MultimodalAgent(this, (_, _) => ValueTask.FromResult(0));
        var harness = new MultimodalAgentHarness(
            [new MultimodalAgentRegistration(name, agent)]);
        var session = harness.CreateSession(
            Guid.NewGuid(), MultimodalHarnessLimits.Unlimited);
        var invocation = new MultimodalAgentInvocationRequest(
            Guid.NewGuid(), name, [], MultimodalAgentRunLimits.Unlimited,
            jsonOutput: requirement);

        var result = await session.InvokeAsync(invocation);

        Assert.Equal(MultimodalAgentInvocationOutcome.Completed, result.Outcome);
        Assert.Same(requirement, LastMultimodalRequest?.JsonOutput);
        var message = Assert.IsType<MultimodalMessage>(
            Assert.Single(result.AgentResult!.Transcript));
        Assert.Equal("{\"ok\":true}",
            Assert.IsType<TextContent>(Assert.Single(message.Contents)).Text);
    }

    public Task<ChatResponse> CompleteAsync(
        ChatRequest request,
        CancellationToken cancellationToken = default)
    {
        LastTextRequest = request;
        return Task.FromResult(new ChatResponse(
            [new ChatCandidate(
                [new ChatMessage(ChatRole.Assistant, "{\"ok\":true}")],
                FinishReason.Stop)]));
    }

    public Task<MultimodalChatResponse> CompleteAsync(
        MultimodalChatRequest request,
        CancellationToken cancellationToken = default)
    {
        LastMultimodalRequest = request;
        return Task.FromResult(new MultimodalChatResponse(
            [new MultimodalChatCandidate(
                [new MultimodalMessage(
                    MultimodalChatRole.Assistant,
                    [new TextContent("{\"ok\":true}")])],
                FinishReason.Stop)]));
    }

    private static JsonOutputRequirement CreateRequirement()
    {
        using var document = JsonDocument.Parse(
            "{\"type\":\"object\",\"properties\":{\"ok\":{\"type\":\"boolean\"}}}");
        return new JsonOutputRequirement(document.RootElement);
    }
}
