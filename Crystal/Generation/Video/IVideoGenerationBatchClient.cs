using Crystal.Generation.Batches;
using Crystal.Generation.Operations;

namespace Crystal.Generation.Video;

/// <summary>Defines optional asynchronous video-generation batch submission.</summary>
public interface IVideoGenerationBatchClient
{
    /// <summary>Gets portable individual generation support.</summary>
    GenerationCapabilities Capabilities { get; }

    /// <summary>Submits one ordered batch without splitting or rewriting it.</summary>
    Task<GenerationOperationSnapshot<GenerationBatchResponse<VideoGenerationResponse>>>
        StartBatchAsync(
            GenerationBatchRequest<VideoGenerationRequest> request,
            CancellationToken cancellationToken = default);

    /// <summary>Polls one previously submitted batch using its latest ticket.</summary>
    Task<GenerationOperationSnapshot<GenerationBatchResponse<VideoGenerationResponse>>>
        PollBatchAsync(
            GenerationOperationTicket ticket,
            CancellationToken cancellationToken = default);
}
