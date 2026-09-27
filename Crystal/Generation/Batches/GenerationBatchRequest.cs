using Crystal.Internal;

namespace Crystal.Generation.Batches;

/// <summary>Contains one ordered batch of target-specific generation requests.</summary>
/// <typeparam name="TRequest">The target-specific request type.</typeparam>
public sealed record GenerationBatchRequest<TRequest>
    where TRequest : class
{
    /// <summary>Initializes a non-empty batch of exact requests.</summary>
    public GenerationBatchRequest(IEnumerable<TRequest> requests)
    {
        Requests = CollectionSnapshot.Create(
            requests,
            nameof(requests),
            allowEmpty: false);
    }

    /// <summary>Gets the requests in submission order.</summary>
    public IReadOnlyList<TRequest> Requests { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(GenerationBatchRequest<TRequest>);
}
