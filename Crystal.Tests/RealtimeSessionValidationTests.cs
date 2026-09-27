using System.Runtime.CompilerServices;
using System.Text.Json;

using Crystal.Decorators;
using Crystal.Multimodal;
using Crystal.Pipelines;
using Crystal.Realtime;
using Crystal.Reasoning;
using Crystal.Tools;

namespace Crystal.Tests;

public sealed class RealtimeSessionValidationTests
{
    [Fact]
    public async Task DeclaredRequirementsOpenTheExactSession()
    {
        var capabilities = TextOnlyCapabilities();
        var request = new RealtimeSessionRequest(
            [ContentModality.Text], RealtimeTurnMode.Automatic);
        var session = new FakeSession();
        using var cancellation = new CancellationTokenSource();
        var pipeline = new AsyncPipeline<RealtimeSessionRequest,
            IRealtimeMediaSession>(
            (actualRequest, token) =>
            {
                Assert.Same(request, actualRequest);
                Assert.Equal(cancellation.Token, token);
                return Task.FromResult<IRealtimeMediaSession>(session);
            },
            [RealtimeSessionValidation.RequireDeclaredCapabilities(capabilities)]);

        var actual = await pipeline.InvokeAsync(request, cancellation.Token);

        Assert.Same(session, actual);
    }

    [Fact]
    public async Task UndeclaredRequirementsFailBeforeOpeningASession()
    {
        using var schema = JsonDocument.Parse("{}");
        var requests = new[]
        {
            new RealtimeSessionRequest(
                [ContentModality.Audio], RealtimeTurnMode.Automatic),
            new RealtimeSessionRequest(
                [ContentModality.Text], RealtimeTurnMode.Explicit),
            new RealtimeSessionRequest(
                [ContentModality.Text], RealtimeTurnMode.Automatic,
                tools: [new ToolDefinition("caller-tool", schema.RootElement)]),
            new RealtimeSessionRequest(
                [ContentModality.Text], RealtimeTurnMode.Automatic,
                reasoning: new ReasoningOptions(effort: ReasoningEffort.Low))
        };
        var opened = false;
        var pipeline = new AsyncPipeline<RealtimeSessionRequest,
            IRealtimeMediaSession>(
            (_, _) =>
            {
                opened = true;
                return Task.FromResult<IRealtimeMediaSession>(new FakeSession());
            },
            [RealtimeSessionValidation.RequireDeclaredCapabilities(
                TextOnlyCapabilities())]);

        foreach (var request in requests)
        {
            var failure = await Assert.ThrowsAsync<ArgumentException>(() =>
                pipeline.InvokeAsync(request));
            Assert.Equal("request", failure.ParamName);
        }

        Assert.False(opened);
    }

    private static RealtimeSessionCapabilities TextOnlyCapabilities()
    {
        var text = new MultimodalContentCapability(ContentModality.Text);
        return new RealtimeSessionCapabilities(
            [text], [text],
            supportsAutomaticTurnDetection: true,
            supportsExplicitTurnEnd: false);
    }

    private sealed class FakeSession : IRealtimeMediaSession
    {
        public Task SendAsync(
            RealtimeInputEvent input,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public async IAsyncEnumerable<RealtimeOutputEvent> ReceiveAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.CompletedTask;
            yield break;
        }

        public Task CloseAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
