using System.Runtime.CompilerServices;

using Crystal.Decorators;
using Crystal.Multimodal;
using Crystal.Realtime;

namespace Crystal.Tests;

public sealed class RealtimeClientDecoratorTests
{
    [Fact]
    public async Task WrapperPreservesSessionAndAppliesOpeningPreflight()
    {
        var text = new MultimodalContentCapability(ContentModality.Text);
        var capabilities = new RealtimeSessionCapabilities(
            [text], [text],
            supportsAutomaticTurnDetection: true,
            supportsExplicitTurnEnd: false);
        var session = new FakeSession();
        var source = new FakeClient(capabilities, session);
        using var cancellation = new CancellationTokenSource();
        var wrapped = Clients.ForRealtimeMedia(source,
            [RealtimeSessionValidation.RequireDeclaredCapabilities(capabilities)]);
        var valid = new RealtimeSessionRequest(
            [ContentModality.Text], RealtimeTurnMode.Automatic);

        var actual = await wrapped.OpenAsync(valid, cancellation.Token);

        Assert.Same(capabilities, wrapped.Capabilities);
        Assert.Same(session, actual);
        Assert.Same(valid, source.Request);
        Assert.Equal(cancellation.Token, source.Token);

        var invalid = new RealtimeSessionRequest(
            [ContentModality.Text], RealtimeTurnMode.Explicit);
        await Assert.ThrowsAsync<ArgumentException>(() =>
            wrapped.OpenAsync(invalid, cancellation.Token));
        Assert.Same(valid, source.Request);
    }

    private sealed class FakeClient(
        RealtimeSessionCapabilities capabilities,
        IRealtimeMediaSession session) : IRealtimeMediaClient
    {
        public RealtimeSessionCapabilities Capabilities { get; } = capabilities;

        public RealtimeSessionRequest? Request { get; private set; }

        public CancellationToken Token { get; private set; }

        public Task<IRealtimeMediaSession> OpenAsync(
            RealtimeSessionRequest request,
            CancellationToken cancellationToken = default)
        {
            Request = request;
            Token = cancellationToken;
            return Task.FromResult(session);
        }
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
            await Task.Yield();
            yield break;
        }

        public Task CloseAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
