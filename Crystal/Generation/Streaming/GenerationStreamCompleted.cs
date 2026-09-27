namespace Crystal.Generation.Streaming;

/// <summary>Ends one stream with the exact complete generation response.</summary>
/// <typeparam name="TResponse">The target-specific response type.</typeparam>
public sealed record GenerationStreamCompleted<TResponse> :
    GenerationStreamEvent<TResponse>
    where TResponse : class
{
    /// <summary>Initializes the terminal stream event.</summary>
    public GenerationStreamCompleted(TResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);
        Response = response;
    }

    /// <summary>Gets the complete authoritative generation response.</summary>
    public TResponse Response { get; }
}
