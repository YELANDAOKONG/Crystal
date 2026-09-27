using Crystal.Generation.Operations;

namespace Crystal.Generation.Audio;

/// <summary>Defines optional resumable remote audio generation.</summary>
public interface IAudioGenerationOperationClient
{
    /// <summary>Gets portable support for audio generation.</summary>
    GenerationCapabilities Capabilities { get; }

    /// <summary>Starts one remote operation from the exact request.</summary>
    /// <remarks>
    /// Cancellation stops local waiting. Once accepted, a remote operation may
    /// continue and the caller must retain the returned ticket.
    /// </remarks>
    Task<GenerationOperationSnapshot<AudioGenerationResponse>> StartAsync(
        AudioGenerationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Obtains one state snapshot using a previously returned ticket.</summary>
    /// <remarks>
    /// The adapter may return a replacement ticket. Cancellation does not cancel
    /// the remote operation.
    /// </remarks>
    Task<GenerationOperationSnapshot<AudioGenerationResponse>> PollAsync(
        GenerationOperationTicket ticket,
        CancellationToken cancellationToken = default);
}
