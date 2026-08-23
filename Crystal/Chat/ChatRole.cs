namespace Crystal.Chat;

/// <summary>
/// Identifies the role of one text-chat message.
/// </summary>
public sealed record ChatRole
{
    /// <summary>
    /// Gets the conventional system role.
    /// </summary>
    public static ChatRole System { get; } = new("system");

    /// <summary>
    /// Gets the conventional user role.
    /// </summary>
    public static ChatRole User { get; } = new("user");

    /// <summary>
    /// Gets the conventional assistant role.
    /// </summary>
    public static ChatRole Assistant { get; } = new("assistant");

    /// <summary>
    /// Initializes a chat role.
    /// </summary>
    /// <param name="value">The role value.</param>
    public ChatRole(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));
        Value = value;
    }

    /// <summary>
    /// Gets the role value.
    /// </summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
