namespace Crystal.Multimodal.Tools;

/// <summary>Configures explicit scheduling for multimodal tool-call batches.</summary>
public sealed record MultimodalToolExecutionOptions
{
    /// <summary>Initializes multimodal tool execution options.</summary>
    /// <param name="mode">The scheduling mode.</param>
    /// <param name="maximumConcurrency">
    /// The positive concurrency bound. Serial mode requires one.
    /// </param>
    public MultimodalToolExecutionOptions(
        MultimodalToolExecutionMode mode,
        int maximumConcurrency)
    {
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(mode),
                mode,
                "Multimodal tool execution mode is not defined.");
        }

        if (maximumConcurrency <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumConcurrency),
                maximumConcurrency,
                "Maximum multimodal tool concurrency must be positive.");
        }

        if (mode == MultimodalToolExecutionMode.Serial
            && maximumConcurrency != 1)
        {
            throw new ArgumentException(
                "Serial multimodal tool execution requires a concurrency value of one.",
                nameof(maximumConcurrency));
        }

        Mode = mode;
        MaximumConcurrency = maximumConcurrency;
    }

    /// <summary>Gets the scheduling mode.</summary>
    public MultimodalToolExecutionMode Mode { get; }

    /// <summary>Gets the concurrency bound.</summary>
    public int MaximumConcurrency { get; }
}
