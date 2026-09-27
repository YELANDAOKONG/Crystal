using Crystal.Generation.Batches;
using Crystal.Generation.Operations;

namespace Crystal.Generation.Images;

/// <summary>Defines optional asynchronous image-generation batch submission.</summary>
public interface IImageGenerationBatchClient
{
    /// <summary>Gets portable individual generation support.</summary>
    GenerationCapabilities Capabilities { get; }

    /// <summary>Submits one ordered batch without splitting or rewriting it.</summary>
    Task<GenerationOperationSnapshot<GenerationBatchResponse<ImageGenerationResponse>>>
        StartBatchAsync(
            GenerationBatchRequest<ImageGenerationRequest> request,
            CancellationToken cancellationToken = default);

    /// <summary>Polls one previously submitted batch using its latest ticket.</summary>
    Task<GenerationOperationSnapshot<GenerationBatchResponse<ImageGenerationResponse>>>
        PollBatchAsync(
            GenerationOperationTicket ticket,
            CancellationToken cancellationToken = default);
}
