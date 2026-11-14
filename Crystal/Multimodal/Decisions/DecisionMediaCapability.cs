using Crystal.Decisions;

namespace Crystal.Multimodal.Decisions;

/// <summary>Declares one supported media modality and its source kinds at one request position.</summary>
public sealed record DecisionMediaCapability
{
    /// <summary>Initializes a supported media input shape.</summary>
    public DecisionMediaCapability(DecisionContentPosition position, MultimodalContentCapability content)
    {
        if (!Enum.IsDefined(position))
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        ArgumentNullException.ThrowIfNull(content);
        if (content.Modality == ContentModality.Text)
        {
            throw new ArgumentException("Text is a decision data capability.", nameof(content));
        }

        Position = position;
        Content = content;
    }

    /// <summary>Gets the supported request position.</summary>
    public DecisionContentPosition Position { get; }

    /// <summary>Gets the supported media modality and source kinds.</summary>
    public MultimodalContentCapability Content { get; }
}
