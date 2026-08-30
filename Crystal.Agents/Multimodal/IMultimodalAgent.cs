using Crystal.Multimodal.Chat;

namespace Crystal.Multimodal.Agents;

/// <summary>Defines one bounded, prompt-free multimodal Agent.</summary>
public interface IMultimodalAgent
{
    /// <summary>Gets the Agent model input and output capabilities.</summary>
    MultimodalChatCapabilities Capabilities { get; }

    /// <summary>Runs the Agent to a terminal result.</summary>
    /// <param name="request">The exact run request.</param>
    /// <param name="cancellationToken">
    /// A token that cancels model, policy, and tool work.
    /// </param>
    /// <returns>The completed run result.</returns>
    Task<MultimodalAgentRunResult> RunAsync(
        MultimodalAgentRunRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Streams typed transitions and a final completion event.</summary>
    /// <param name="request">The exact run request.</param>
    /// <param name="cancellationToken">
    /// A token that cancels enumeration and in-flight work.
    /// </param>
    /// <returns>The ordered run events.</returns>
    IAsyncEnumerable<MultimodalAgentRunEvent> StreamAsync(
        MultimodalAgentRunRequest request,
        CancellationToken cancellationToken = default);
}
