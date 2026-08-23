namespace Crystal.Chat;

/// <summary>
/// Carries provider-reported usage for a text-chat stream.
/// </summary>
public sealed record ChatUsageReceived : ChatStreamEvent
{
    /// <summary>
    /// Initializes a usage event.
    /// </summary>
    /// <param name="usage">The provider-reported usage.</param>
    public ChatUsageReceived(TokenUsage usage)
    {
        ArgumentNullException.ThrowIfNull(usage, nameof(usage));
        Usage = usage;
    }

    /// <summary>
    /// Gets the provider-reported usage.
    /// </summary>
    public TokenUsage Usage { get; }
}
