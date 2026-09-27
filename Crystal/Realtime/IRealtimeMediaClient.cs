namespace Crystal.Realtime;

/// <summary>Opens optional provider-neutral duplex media sessions.</summary>
public interface IRealtimeMediaClient
{
    /// <summary>Gets portable live-session capabilities.</summary>
    RealtimeSessionCapabilities Capabilities { get; }

    /// <summary>Opens one independent live session with exact requirements.</summary>
    Task<IRealtimeMediaSession> OpenAsync(
        RealtimeSessionRequest request,
        CancellationToken cancellationToken = default);
}
