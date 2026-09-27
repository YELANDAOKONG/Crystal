namespace Crystal.Generation.Operations;

/// <summary>
/// Captures one adapter-reported state of a remote generation operation.
/// </summary>
/// <typeparam name="TResponse">The target-specific complete response type.</typeparam>
public sealed record GenerationOperationSnapshot<TResponse>
    where TResponse : class
{
    /// <summary>Initializes an operation snapshot.</summary>
    /// <param name="ticket">The ticket to retain for a later poll.</param>
    /// <param name="status">The portable remote operation status.</param>
    /// <param name="response">The exact complete response, only when completed.</param>
    public GenerationOperationSnapshot(
        GenerationOperationTicket ticket,
        GenerationOperationStatus status,
        TResponse? response = null)
    {
        ArgumentNullException.ThrowIfNull(ticket);

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        if ((status == GenerationOperationStatus.Completed) != (response is not null))
        {
            throw new ArgumentException(
                "A response is required exactly when an operation completes.",
                nameof(response));
        }

        Ticket = ticket;
        Status = status;
        Response = response;
    }

    /// <summary>Gets the ticket to retain for a later poll.</summary>
    public GenerationOperationTicket Ticket { get; }

    /// <summary>Gets the portable remote operation status.</summary>
    public GenerationOperationStatus Status { get; }

    /// <summary>Gets the complete response when the operation has completed.</summary>
    public TResponse? Response { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(GenerationOperationSnapshot<TResponse>);
}
