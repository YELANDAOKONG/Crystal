namespace Crystal;

/// <summary>
/// Describes provider-reported token consumption for one operation.
/// </summary>
public sealed record TokenUsage
{
    /// <summary>
    /// Initializes token usage.
    /// </summary>
    /// <param name="inputTokenCount">The number of input tokens.</param>
    /// <param name="outputTokenCount">The number of output tokens.</param>
    /// <param name="reasoningTokenCount">
    /// The provider-reported reasoning-token count, when available.
    /// </param>
    public TokenUsage(
        long inputTokenCount,
        long outputTokenCount,
        long? reasoningTokenCount = null)
    {
        if (inputTokenCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(inputTokenCount),
                inputTokenCount,
                "Input token count cannot be negative.");
        }

        if (outputTokenCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(outputTokenCount),
                outputTokenCount,
                "Output token count cannot be negative.");
        }

        if (reasoningTokenCount is < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(reasoningTokenCount),
                reasoningTokenCount,
                "Reasoning token count cannot be negative.");
        }

        InputTokenCount = inputTokenCount;
        OutputTokenCount = outputTokenCount;
        ReasoningTokenCount = reasoningTokenCount;
        TotalTokenCount = checked(inputTokenCount + outputTokenCount);
    }

    /// <summary>
    /// Gets the input-token count.
    /// </summary>
    public long InputTokenCount { get; }

    /// <summary>
    /// Gets the output-token count.
    /// </summary>
    public long OutputTokenCount { get; }

    /// <summary>
    /// Gets the reasoning-token count when the provider reports it.
    /// </summary>
    public long? ReasoningTokenCount { get; }

    /// <summary>
    /// Gets the sum of input and output tokens.
    /// </summary>
    public long TotalTokenCount { get; }
}
