using Crystal.Decisions;
using Crystal.Internal;

namespace Crystal.Multimodal.Decisions;

/// <summary>Declares individual question, data, and media input shapes.</summary>
public sealed record MultimodalDecisionCapabilities
{
    /// <summary>Initializes multimodal decision capabilities.</summary>
    /// <param name="decisions">The supported question types and non-media input shapes.</param>
    /// <param name="media">The supported media input shapes by position.</param>
    public MultimodalDecisionCapabilities(
        DecisionCapabilities decisions,
        IEnumerable<DecisionMediaCapability> media)
    {
        ArgumentNullException.ThrowIfNull(decisions);
        Decisions = decisions;
        Media = CollectionSnapshot.Create(media, nameof(media));

        if (Media.Select(static entry => (entry.Position, entry.Content.Modality)).Distinct().Count()
            != Media.Count)
        {
            throw new ArgumentException("Decision media capability pairs must be unique.", nameof(media));
        }
    }

    /// <summary>Gets the supported question types and non-media input shapes.</summary>
    public DecisionCapabilities Decisions { get; }

    /// <summary>Gets the supported media input shapes by position.</summary>
    public IReadOnlyList<DecisionMediaCapability> Media { get; }
}
