namespace Crystal.Multimodal.Chat;

/// <summary>Identifies the role of one multimodal message.</summary>
public sealed record MultimodalChatRole
{
    /// <summary>Gets the conventional system role.</summary>
    public static MultimodalChatRole System { get; } = new("system");

    /// <summary>Gets the conventional user role.</summary>
    public static MultimodalChatRole User { get; } = new("user");

    /// <summary>Gets the conventional assistant role.</summary>
    public static MultimodalChatRole Assistant { get; } = new("assistant");

    /// <summary>Initializes a multimodal chat role.</summary>
    /// <param name="value">The complete provider-neutral or provider role.</param>
    public MultimodalChatRole(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));
        Value = value;
    }

    /// <summary>Gets the complete role value.</summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
