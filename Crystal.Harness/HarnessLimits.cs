namespace Crystal.Harness;

/// <summary>
/// Defines finite shared limits for one Harness session.
/// </summary>
public sealed record HarnessLimits
{
    private static readonly TimeSpan MaximumSupportedDuration =
        TimeSpan.FromMilliseconds(uint.MaxValue - 1);

    /// <summary>
    /// Initializes Harness session limits.
    /// </summary>
    /// <param name="maximumDepth">
    /// The non-negative maximum invocation depth, with the root at zero.
    /// </param>
    /// <param name="maximumModelCalls">
    /// The positive shared maximum attempted model calls.
    /// </param>
    /// <param name="maximumToolCalls">
    /// The non-negative shared maximum attempted tool calls.
    /// </param>
    /// <param name="maximumDuration">
    /// The positive finite shared wall-clock duration.
    /// </param>
    public HarnessLimits(
        int maximumDepth,
        int maximumModelCalls,
        int maximumToolCalls,
        TimeSpan maximumDuration)
    {
        if (maximumDepth < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumDepth),
                maximumDepth,
                "Maximum Harness depth cannot be negative.");
        }

        if (maximumModelCalls <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumModelCalls),
                maximumModelCalls,
                "Maximum Harness model calls must be positive.");
        }

        if (maximumToolCalls < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumToolCalls),
                maximumToolCalls,
                "Maximum Harness tool calls cannot be negative.");
        }

        if (maximumDuration <= TimeSpan.Zero
            || maximumDuration == Timeout.InfiniteTimeSpan
            || maximumDuration > MaximumSupportedDuration)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumDuration),
                maximumDuration,
                "Maximum Harness duration must be positive, finite, and supported by the runtime timer.");
        }

        MaximumDepth = maximumDepth;
        MaximumModelCalls = maximumModelCalls;
        MaximumToolCalls = maximumToolCalls;
        MaximumDuration = maximumDuration;
    }

    /// <summary>
    /// Gets the maximum invocation depth.
    /// </summary>
    public int MaximumDepth { get; }

    /// <summary>
    /// Gets the shared maximum attempted model calls.
    /// </summary>
    public int MaximumModelCalls { get; }

    /// <summary>
    /// Gets the shared maximum attempted tool calls.
    /// </summary>
    public int MaximumToolCalls { get; }

    /// <summary>
    /// Gets the shared wall-clock duration.
    /// </summary>
    public TimeSpan MaximumDuration { get; }
}
