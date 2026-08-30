using Crystal.Internal;

namespace Crystal.Multimodal.Chat;

/// <summary>Contains one ordered multimodal message with an explicit role.</summary>
public sealed record MultimodalMessage : MultimodalChatItem
{
    /// <summary>Initializes a multimodal message.</summary>
    /// <param name="role">The explicit message role.</param>
    /// <param name="contents">The ordered typed content blocks.</param>
    public MultimodalMessage(
        MultimodalChatRole role,
        IEnumerable<MultimodalContent> contents)
    {
        ArgumentNullException.ThrowIfNull(role, nameof(role));

        Role = role;
        Contents = CollectionSnapshot.Create(contents, nameof(contents));
    }

    /// <summary>Gets the explicit message role.</summary>
    public MultimodalChatRole Role { get; }

    /// <summary>Gets the ordered typed content blocks.</summary>
    public IReadOnlyList<MultimodalContent> Contents { get; }
}
