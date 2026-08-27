namespace Crystal.Tools;

/// <summary>
/// Configures explicit scheduling for tool-call batches.
/// </summary>
public sealed record ToolExecutionOptions
{
    /// <summary>
    /// Initializes tool execution options.
    /// </summary>
    /// <param name="mode">The scheduling mode.</param>
    /// <param name="maximumConcurrency">
    /// The positive concurrency bound. Serial mode requires one.
    /// </param>
    public ToolExecutionOptions(
        ToolExecutionMode mode,
        int maximumConcurrency)
    {
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(mode),
                mode,
                "Tool execution mode is not defined.");
        }

        if (maximumConcurrency <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumConcurrency),
                maximumConcurrency,
                "Maximum tool concurrency must be positive.");
        }

        if (mode == ToolExecutionMode.Serial && maximumConcurrency != 1)
        {
            throw new ArgumentException(
                "Serial tool execution requires a concurrency value of one.",
                nameof(maximumConcurrency));
        }

        Mode = mode;
        MaximumConcurrency = maximumConcurrency;
    }

    /// <summary>
    /// Gets the scheduling mode.
    /// </summary>
    public ToolExecutionMode Mode { get; }

    /// <summary>
    /// Gets the concurrency bound.
    /// </summary>
    public int MaximumConcurrency { get; }
}
