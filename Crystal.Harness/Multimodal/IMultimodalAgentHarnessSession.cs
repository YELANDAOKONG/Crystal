namespace Crystal.Multimodal.Harness;

/// <summary>Defines one bounded multimodal Agent Harness session.</summary>
public interface IMultimodalAgentHarnessSession
{
    /// <summary>Gets the session identifier.</summary>
    Guid SessionId { get; }

    /// <summary>Gets the finite shared session limits.</summary>
    MultimodalHarnessLimits Limits { get; }

    /// <summary>Invokes one registered multimodal Agent.</summary>
    /// <param name="request">The explicit invocation request.</param>
    /// <param name="cancellationToken">A token that cancels this invocation.</param>
    /// <returns>The Harness invocation result.</returns>
    Task<MultimodalAgentInvocationResult> InvokeAsync(
        MultimodalAgentInvocationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Streams an invocation and exact forwarded Agent events.</summary>
    /// <param name="request">The explicit invocation request.</param>
    /// <param name="cancellationToken">
    /// A token that cancels enumeration and the invocation.
    /// </param>
    /// <returns>The ordered multimodal Harness events.</returns>
    IAsyncEnumerable<MultimodalHarnessEvent> StreamAsync(
        MultimodalAgentInvocationRequest request,
        CancellationToken cancellationToken = default);
}
