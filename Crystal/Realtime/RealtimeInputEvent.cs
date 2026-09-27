namespace Crystal.Realtime;

/// <summary>Represents one caller-owned live-session input event.</summary>
public abstract record RealtimeInputEvent
{
    private protected RealtimeInputEvent()
    {
    }

    /// <inheritdoc />
    public sealed override string ToString() => GetType().Name;
}
