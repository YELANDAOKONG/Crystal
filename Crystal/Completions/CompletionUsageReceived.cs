namespace Crystal.Completions;

/// <summary>
/// Carries provider-reported usage for a completion stream.
/// </summary>
public sealed record CompletionUsageReceived : CompletionStreamEvent
{
    /// <summary>
    /// Initializes a usage event.
    /// </summary>
    /// <param name="usage">The provider-reported usage.</param>
    public CompletionUsageReceived(TokenUsage usage)
    {
        ArgumentNullException.ThrowIfNull(usage, nameof(usage));
        Usage = usage;
    }

    /// <summary>
    /// Gets the provider-reported usage.
    /// </summary>
    public TokenUsage Usage { get; }
}
