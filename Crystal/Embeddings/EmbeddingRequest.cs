using Crystal.Internal;

namespace Crystal.Embeddings;

/// <summary>
/// Contains an ordered batch of text inputs to embed.
/// </summary>
public sealed record EmbeddingRequest
{
    /// <summary>
    /// Initializes an embedding request.
    /// </summary>
    /// <param name="inputs">The non-empty ordered text batch.</param>
    public EmbeddingRequest(IEnumerable<string> inputs)
    {
        Inputs = CollectionSnapshot.Create(
            inputs,
            nameof(inputs),
            allowEmpty: false);
    }

    /// <summary>
    /// Gets the ordered text inputs.
    /// </summary>
    public IReadOnlyList<string> Inputs { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(EmbeddingRequest);
}
