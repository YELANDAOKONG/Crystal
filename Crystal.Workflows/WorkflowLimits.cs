namespace Crystal.Workflows;

/// <summary>Sets explicit graph execution bounds.</summary>
public sealed record WorkflowLimits
{
    /// <summary>Gets an unlimited-step, serial execution setting.</summary>
    public static WorkflowLimits Unlimited { get; } = new();

    /// <summary>Initializes workflow limits.</summary>
    /// <param name="maximumSupersteps">An optional positive step bound.</param>
    /// <param name="maximumConcurrency">The positive concurrent node bound.</param>
    public WorkflowLimits(
        int? maximumSupersteps = null,
        int maximumConcurrency = 1)
    {
        if (maximumSupersteps is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumSupersteps),
                maximumSupersteps,
                "Maximum supersteps must be positive when configured.");
        }

        if (maximumConcurrency <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumConcurrency),
                maximumConcurrency,
                "Maximum concurrency must be positive.");
        }

        MaximumSupersteps = maximumSupersteps;
        MaximumConcurrency = maximumConcurrency;
    }

    /// <summary>Gets the optional step limit; null means unlimited.</summary>
    public int? MaximumSupersteps { get; }

    /// <summary>Gets the maximum number of concurrently invoked nodes.</summary>
    public int MaximumConcurrency { get; }
}
