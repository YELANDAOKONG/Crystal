namespace Crystal.Multimodal.Chat;

/// <summary>Carries provider-reported usage for a multimodal Chat stream.</summary>
public sealed record MultimodalChatUsageReceived : MultimodalChatStreamEvent
{
    /// <summary>Initializes a usage event.</summary>
    /// <param name="usage">The provider-reported usage.</param>
    public MultimodalChatUsageReceived(TokenUsage usage)
    {
        ArgumentNullException.ThrowIfNull(usage, nameof(usage));
        Usage = usage;
    }

    /// <summary>Gets the provider-reported usage.</summary>
    public TokenUsage Usage { get; }
}
