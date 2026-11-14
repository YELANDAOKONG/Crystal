namespace Crystal.Decisions.Results;

/// <summary>Reports that the whole request was blocked before answers were returned.</summary>
public sealed record DecisionBlock
{
    /// <summary>Initializes a request block with optional service-reported details.</summary>
    public DecisionBlock(string? reason = null, string? message = null)
    {
        if (reason is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        }

        Reason = reason;
        Message = message;
    }

    /// <summary>Gets the exact open service-reported reason when available.</summary>
    public string? Reason { get; }

    /// <summary>Gets the exact service-reported detail when available.</summary>
    public string? Message { get; }

    /// <inheritdoc />
    public override string ToString() => nameof(DecisionBlock);
}
