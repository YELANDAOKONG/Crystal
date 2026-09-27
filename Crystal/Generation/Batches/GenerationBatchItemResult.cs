namespace Crystal.Generation.Batches;

/// <summary>Contains one terminal result at its original batch index.</summary>
/// <typeparam name="TResponse">The target-specific response type.</typeparam>
public sealed record GenerationBatchItemResult<TResponse>
    where TResponse : class
{
    /// <summary>Initializes one terminal batch item result.</summary>
    public GenerationBatchItemResult(
        GenerationBatchItemStatus status,
        TResponse? response = null)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        if ((status == GenerationBatchItemStatus.Completed) != (response is not null))
        {
            throw new ArgumentException(
                "A response is required exactly when a batch item completes.",
                nameof(response));
        }

        Status = status;
        Response = response;
    }

    /// <summary>Gets the terminal state of this item.</summary>
    public GenerationBatchItemStatus Status { get; }

    /// <summary>Gets the complete response when this item completed.</summary>
    public TResponse? Response { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(GenerationBatchItemResult<TResponse>);
}
