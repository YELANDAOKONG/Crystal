using Crystal.Generation.Operations;

namespace Crystal.Generation.Images;

/// <summary>Defines optional resumable remote image generation.</summary>
public interface IImageGenerationOperationClient
{
    /// <summary>Gets portable support for image generation.</summary>
    GenerationCapabilities Capabilities { get; }

    /// <summary>Starts one remote operation from the exact request.</summary>
    /// <remarks>
    /// Cancellation stops local waiting. Once accepted, a remote operation may
    /// continue and the caller must retain the returned ticket.
    /// </remarks>
    Task<GenerationOperationSnapshot<ImageGenerationResponse>> StartAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Obtains one state snapshot using a previously returned ticket.</summary>
    /// <remarks>
    /// The adapter may return a replacement ticket. Cancellation does not cancel
    /// the remote operation.
    /// </remarks>
    Task<GenerationOperationSnapshot<ImageGenerationResponse>> PollAsync(
        GenerationOperationTicket ticket,
        CancellationToken cancellationToken = default);
}
