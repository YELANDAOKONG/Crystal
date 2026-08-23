namespace Crystal.Reasoning;

/// <summary>
/// Contains portable semantic hints for model reasoning.
/// </summary>
public sealed record ReasoningOptions
{
    /// <summary>
    /// Initializes reasoning options.
    /// </summary>
    /// <param name="mode">The requested reasoning mode.</param>
    /// <param name="effort">The requested relative effort.</param>
    /// <param name="output">The requested readable output surface.</param>
    /// <param name="tokenBudget">An optional positive reasoning-token budget.</param>
    public ReasoningOptions(
        ReasoningMode? mode = null,
        ReasoningEffort? effort = null,
        ReasoningOutput? output = null,
        int? tokenBudget = null)
    {
        if (tokenBudget is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tokenBudget),
                tokenBudget,
                "Reasoning token budget must be positive.");
        }

        Mode = mode;
        Effort = effort;
        Output = output;
        TokenBudget = tokenBudget;
    }

    /// <summary>
    /// Gets the requested reasoning mode.
    /// </summary>
    public ReasoningMode? Mode { get; }

    /// <summary>
    /// Gets the requested effort.
    /// </summary>
    public ReasoningEffort? Effort { get; }

    /// <summary>
    /// Gets the requested readable output.
    /// </summary>
    public ReasoningOutput? Output { get; }

    /// <summary>
    /// Gets the requested reasoning-token budget.
    /// </summary>
    public int? TokenBudget { get; }
}
