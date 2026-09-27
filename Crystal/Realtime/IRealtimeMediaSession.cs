namespace Crystal.Realtime;

/// <summary>
/// Defines one live duplex exchange. The caller serializes sends while one
/// receive enumeration may overlap them.
/// </summary>
public interface IRealtimeMediaSession
{
    /// <summary>Sends one exact caller-owned event in call order.</summary>
    Task SendAsync(
        RealtimeInputEvent input,
        CancellationToken cancellationToken = default);

    /// <summary>Receives exact remote events in order through one enumeration.</summary>
    IAsyncEnumerable<RealtimeOutputEvent> ReceiveAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Closes the local session and releases its resources.</summary>
    Task CloseAsync(CancellationToken cancellationToken = default);
}
