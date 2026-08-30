using Crystal.Internal;

namespace Crystal.Generation;

/// <summary>Contains one ordered immediate-generation candidate.</summary>
public sealed record GenerationCandidate
{
    /// <summary>Initializes an immediate-generation candidate.</summary>
    /// <param name="items">The exact ordered output items.</param>
    /// <param name="finishReason">
    /// The provider-reported finish reason when available.
    /// </param>
    public GenerationCandidate(
        IEnumerable<GenerationItem> items,
        FinishReason? finishReason = null)
    {
        Items = CollectionSnapshot.Create(items, nameof(items));
        FinishReason = finishReason;
    }

    /// <summary>Gets the exact ordered output items.</summary>
    public IReadOnlyList<GenerationItem> Items { get; }

    /// <summary>Gets the provider-reported finish reason when available.</summary>
    public FinishReason? FinishReason { get; }
}
