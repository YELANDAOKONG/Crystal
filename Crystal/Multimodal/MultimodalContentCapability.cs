using Crystal.Internal;
using Crystal.Media;

namespace Crystal.Multimodal;

/// <summary>
/// Describes one supported content modality and its media source shapes.
/// </summary>
public sealed record MultimodalContentCapability
{
    /// <summary>Initializes a content capability.</summary>
    /// <param name="modality">The supported modality.</param>
    /// <param name="sourceKinds">
    /// Supported source shapes for media. Text requires an empty collection.
    /// </param>
    public MultimodalContentCapability(
        ContentModality modality,
        IEnumerable<MediaSourceKind>? sourceKinds = null)
    {
        ArgumentNullException.ThrowIfNull(modality, nameof(modality));

        var sourceKindSnapshot = CollectionSnapshot.Create(
            sourceKinds ?? Array.Empty<MediaSourceKind>(),
            nameof(sourceKinds));

        if (sourceKindSnapshot.Distinct().Count() != sourceKindSnapshot.Count)
        {
            throw new ArgumentException(
                "Media source kinds must be unique.",
                nameof(sourceKinds));
        }

        if (modality == ContentModality.Text && sourceKindSnapshot.Count != 0)
        {
            throw new ArgumentException(
                "Text content cannot declare media source kinds.",
                nameof(sourceKinds));
        }

        if (modality != ContentModality.Text && sourceKindSnapshot.Count == 0)
        {
            throw new ArgumentException(
                "Media content must declare at least one source kind.",
                nameof(sourceKinds));
        }

        Modality = modality;
        SourceKinds = sourceKindSnapshot;
    }

    /// <summary>Gets the supported modality.</summary>
    public ContentModality Modality { get; }

    /// <summary>Gets supported media source shapes.</summary>
    public IReadOnlyList<MediaSourceKind> SourceKinds { get; }
}
