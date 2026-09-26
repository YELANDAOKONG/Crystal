namespace Crystal.Pipelines;

/// <summary>Reports content-free timing and status for one pipeline invocation.</summary>
public sealed record PipelineObservation
{
    /// <summary>Initializes an observation without request, response, or exception data.</summary>
    /// <param name="operationName">The caller-supplied stable operation name.</param>
    /// <param name="status">The invocation status.</param>
    /// <param name="elapsed">Elapsed time since the invocation started.</param>
    public PipelineObservation(
        string operationName,
        PipelineStatus status,
        TimeSpan elapsed)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName, nameof(operationName));

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "Invalid pipeline status.");
        }

        if (elapsed < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(elapsed), elapsed, "Elapsed time cannot be negative.");
        }

        OperationName = operationName;
        Status = status;
        Elapsed = elapsed;
    }

    /// <summary>Gets the caller-supplied stable operation name.</summary>
    public string OperationName { get; }

    /// <summary>Gets the invocation status.</summary>
    public PipelineStatus Status { get; }

    /// <summary>Gets elapsed time since invocation start.</summary>
    public TimeSpan Elapsed { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(PipelineObservation);
}
