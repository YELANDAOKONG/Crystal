namespace Crystal.Internal;

internal sealed class AgentOperationResult<T>
{
    private AgentOperationResult(
        bool timedOut,
        T value)
    {
        TimedOut = timedOut;
        Value = value;
    }

    public bool TimedOut { get; }

    public T Value { get; }

    public static AgentOperationResult<T> Success(T value) =>
        new(false, value);

    public static AgentOperationResult<T> Timeout() =>
        new(true, default!);
}
