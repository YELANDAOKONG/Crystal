namespace Crystal.Multimodal.Agents;

/// <summary>Defines optional limits for one multimodal Agent run.</summary>
public sealed record MultimodalAgentRunLimits
{
    private static readonly TimeSpan MaximumSupportedDuration =
        TimeSpan.FromMilliseconds(uint.MaxValue - 1);

    /// <summary>Gets a multimodal Agent run with no configured budget limits.</summary>
    public static MultimodalAgentRunLimits Unlimited { get; } = new(null, null, null);

    /// <summary>Initializes multimodal Agent run limits.</summary>
    /// <param name="maximumModelCalls">
    /// The positive maximum number of attempted model calls, or null for no limit.
    /// </param>
    /// <param name="maximumToolCalls">
    /// The non-negative maximum number of attempted tool calls, or null for no limit.
    /// </param>
    /// <param name="maximumDuration">
    /// The positive finite maximum wall-clock duration, or null for no limit.
    /// </param>
    public MultimodalAgentRunLimits(
        int? maximumModelCalls,
        int? maximumToolCalls,
        TimeSpan? maximumDuration)
    {
        if (maximumModelCalls is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumModelCalls),
                maximumModelCalls,
                "Maximum model calls must be positive.");
        }

        if (maximumToolCalls is < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumToolCalls),
                maximumToolCalls,
                "Maximum tool calls cannot be negative.");
        }

        if (maximumDuration is TimeSpan duration
            && (duration <= TimeSpan.Zero
                || duration > MaximumSupportedDuration))
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

    /// <summary>Gets the maximum attempted model calls, or null when unlimited.</summary>
    public int? MaximumModelCalls { get; }

    /// <summary>Gets the maximum attempted tool calls, or null when unlimited.</summary>
    public int? MaximumToolCalls { get; }

    /// <summary>Gets the maximum wall-clock duration, or null when unlimited.</summary>
    public TimeSpan? MaximumDuration { get; }
}
