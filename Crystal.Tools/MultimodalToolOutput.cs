using Crystal.Internal;

namespace Crystal.Multimodal.Tools;

/// <summary>Contains exact caller-owned multimodal tool output.</summary>
public sealed record MultimodalToolOutput
{
    /// <summary>Initializes successful multimodal tool output.</summary>
    /// <param name="contents">The ordered caller-owned content.</param>
    public MultimodalToolOutput(IEnumerable<MultimodalContent> contents)
        : this(contents, MultimodalToolResultStatus.Success)
    {
    }

    /// <summary>Initializes multimodal tool output.</summary>
    /// <param name="contents">The ordered caller-owned content.</param>
    /// <param name="status">The caller-declared output status.</param>
    public MultimodalToolOutput(
        IEnumerable<MultimodalContent> contents,
        MultimodalToolResultStatus status)
    {
        ArgumentNullException.ThrowIfNull(status, nameof(status));

        Contents = CollectionSnapshot.Create(contents, nameof(contents));
        Status = status;
    }

    /// <summary>Gets the ordered caller-owned content.</summary>
    public IReadOnlyList<MultimodalContent> Contents { get; }

    /// <summary>Gets the caller-declared output status.</summary>
    public MultimodalToolResultStatus Status { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(MultimodalToolOutput);
}
