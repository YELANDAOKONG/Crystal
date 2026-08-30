namespace Crystal.Multimodal.Agents;

/// <summary>Defines finite limits for one multimodal Agent run.</summary>
public sealed record MultimodalAgentRunLimits
{
    private static readonly TimeSpan MaximumSupportedDuration =
        TimeSpan.FromMilliseconds(uint.MaxValue - 1);

    /// <summary>Initializes multimodal Agent run limits.</summary>
    /// <param name="maximumModelCalls">
    /// The positive maximum number of attempted model calls.
    /// </param>
    /// <param name="maximumToolCalls">
    /// The non-negative maximum number of attempted tool calls.
    /// </param>
    /// <param name="maximumDuration">
    /// The positive finite maximum wall-clock duration.
    /// </param>
    public MultimodalAgentRunLimits(
        int maximumModelCalls,
        int maximumToolCalls,
        TimeSpan maximumDuration)
    {
        if (maximumModelCalls <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumModelCalls),
                maximumModelCalls,
                "Maximum model calls must be positive.");
        }

        if (maximumToolCalls < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumToolCalls),
                maximumToolCalls,
                "Maximum tool calls cannot be negative.");
        }

        if (maximumDuration <= TimeSpan.Zero
            || maximumDuration == Timeout.InfiniteTimeSpan
            || maximumDuration > MaximumSupportedDuration)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumDuration),
                maximumDuration,
                "Maximum duration must be positive, finite, and supported by the runtime timer.");
        }

        MaximumModelCalls = maximumModelCalls;
        MaximumToolCalls = maximumToolCalls;
        MaximumDuration = maximumDuration;
    }

    /// <summary>Gets the maximum attempted model calls.</summary>
    public int MaximumModelCalls { get; }

    /// <summary>Gets the maximum attempted tool calls.</summary>
    public int MaximumToolCalls { get; }

    /// <summary>Gets the maximum wall-clock duration.</summary>
    public TimeSpan MaximumDuration { get; }
}
