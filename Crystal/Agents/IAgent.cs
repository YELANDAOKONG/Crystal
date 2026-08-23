namespace Crystal.Agents;

/// <summary>
/// Defines one bounded, prompt-free text Agent.
/// </summary>
public interface IAgent
{
    /// <summary>
    /// Runs the Agent to a terminal result.
    /// </summary>
    /// <param name="request">The exact run request.</param>
    /// <param name="cancellationToken">
    /// A token that cancels model, policy, and tool work.
    /// </param>
    /// <returns>The completed run result.</returns>
    Task<AgentRunResult> RunAsync(
        AgentRunRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams typed run transitions and a final completion event.
    /// </summary>
    /// <param name="request">The exact run request.</param>
    /// <param name="cancellationToken">
    /// A token that cancels enumeration and in-flight work.
    /// </param>
    /// <returns>The ordered run events.</returns>
    IAsyncEnumerable<AgentRunEvent> StreamAsync(
        AgentRunRequest request,
        CancellationToken cancellationToken = default);
}
