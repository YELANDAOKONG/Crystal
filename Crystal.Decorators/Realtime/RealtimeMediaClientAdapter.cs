using Crystal.Pipelines;
using Crystal.Realtime;

namespace Crystal.Decorators.Realtime;

internal sealed class RealtimeMediaClientAdapter : IRealtimeMediaClient
{
    private readonly AsyncPipeline<RealtimeSessionRequest, IRealtimeMediaSession> _open;

    public RealtimeMediaClientAdapter(
        IRealtimeMediaClient client,
        IEnumerable<AsyncMiddleware<RealtimeSessionRequest,
            IRealtimeMediaSession>> middleware)
    {
        ArgumentNullException.ThrowIfNull(client);
        Capabilities = client.Capabilities;
        _open = new(client.OpenAsync, middleware);
    }

    public RealtimeSessionCapabilities Capabilities { get; }

    public Task<IRealtimeMediaSession> OpenAsync(
        RealtimeSessionRequest request,
        CancellationToken cancellationToken = default) =>
        _open.InvokeAsync(request, cancellationToken);
}
