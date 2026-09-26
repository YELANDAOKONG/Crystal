namespace Crystal.Multimodal.Harness;

/// <summary>Defines optional shared limits for a multimodal Harness session.</summary>
public sealed record MultimodalHarnessLimits
{
    private static readonly TimeSpan MaximumSupportedDuration =
        TimeSpan.FromMilliseconds(uint.MaxValue - 1);

    /// <summary>Gets a multimodal Harness session with no configured budget limits.</summary>
    public static MultimodalHarnessLimits Unlimited { get; } = new(null, null, null, null);

    /// <summary>Initializes multimodal Harness limits.</summary>
    /// <param name="maximumDepth">
    /// The non-negative maximum invocation depth, with the root at zero, or null for no limit.
    /// </param>
    /// <param name="maximumModelCalls">
    /// The positive shared maximum attempted model calls, or null for no limit.
    /// </param>
    /// <param name="maximumToolCalls">
    /// The non-negative shared maximum attempted tool calls, or null for no limit.
    /// </param>
    /// <param name="maximumDuration">
    /// The positive finite shared wall-clock duration, or null for no limit.
    /// </param>
    public MultimodalHarnessLimits(
        int? maximumDepth,
        int? maximumModelCalls,
        int? maximumToolCalls,
        TimeSpan? maximumDuration)
    {
        if (maximumDepth is < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumDepth),
                maximumDepth,
                "Maximum multimodal Harness depth cannot be negative.");
        }

        if (maximumModelCalls is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumModelCalls),
                maximumModelCalls,
                "Maximum multimodal Harness model calls must be positive.");
        }

        if (maximumToolCalls is < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumToolCalls),
                maximumToolCalls,
                "Maximum multimodal Harness tool calls cannot be negative.");
        }

        if (maximumDuration is TimeSpan duration
            && (duration <= TimeSpan.Zero
                || duration > MaximumSupportedDuration))
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumDuration),
                maximumDuration,
                "Maximum multimodal Harness duration must be positive, "
                + "finite, and supported by the runtime timer.");
        }

        MaximumDepth = maximumDepth;
        MaximumModelCalls = maximumModelCalls;
        MaximumToolCalls = maximumToolCalls;
        MaximumDuration = maximumDuration;
    }

    /// <summary>Gets the maximum invocation depth, or null when unlimited.</summary>
    public int? MaximumDepth { get; }

    /// <summary>Gets the shared maximum attempted model calls, or null when unlimited.</summary>
    public int? MaximumModelCalls { get; }

    /// <summary>Gets the shared maximum attempted tool calls, or null when unlimited.</summary>
    public int? MaximumToolCalls { get; }

    /// <summary>Gets the shared wall-clock duration, or null when unlimited.</summary>
    public TimeSpan? MaximumDuration { get; }
}
