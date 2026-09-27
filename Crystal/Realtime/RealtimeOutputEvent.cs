namespace Crystal.Realtime;

/// <summary>Represents one adapter-reported live-session output event.</summary>
public abstract record RealtimeOutputEvent
{
    private protected RealtimeOutputEvent()
    {
    }

    /// <inheritdoc />
    public sealed override string ToString() => GetType().Name;
}
