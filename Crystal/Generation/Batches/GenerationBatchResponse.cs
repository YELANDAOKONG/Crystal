using Crystal.Internal;

namespace Crystal.Generation.Batches;

/// <summary>
/// Contains exactly one ordered terminal item result per submitted request.
/// </summary>
/// <typeparam name="TResponse">The target-specific response type.</typeparam>
public sealed record GenerationBatchResponse<TResponse>
    where TResponse : class
{
    /// <summary>Initializes a completed batch response.</summary>
    /// <param name="inputCount">The positive submitted request count.</param>
    /// <param name="items">The corresponding ordered terminal item results.</param>
    public GenerationBatchResponse(
        int inputCount,
        IEnumerable<GenerationBatchItemResult<TResponse>> items)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(inputCount);

        var snapshot = CollectionSnapshot.Create(items, nameof(items));
        if (snapshot.Count != inputCount)
        {
            throw new ArgumentException(
                "Batch item count must equal the submitted request count.",
                nameof(items));
        }

        InputCount = inputCount;
        Items = snapshot;
    }

    /// <summary>Gets the submitted request count.</summary>
    public int InputCount { get; }

    /// <summary>Gets one item result for each request in original order.</summary>
    public IReadOnlyList<GenerationBatchItemResult<TResponse>> Items { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(GenerationBatchResponse<TResponse>);
}
