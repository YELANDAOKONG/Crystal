using Crystal.Generation.Batches;
using Crystal.Generation.Operations;

namespace Crystal.Generation.Audio;

/// <summary>Defines optional asynchronous audio-generation batch submission.</summary>
public interface IAudioGenerationBatchClient
{
    /// <summary>Gets portable individual generation support.</summary>
    GenerationCapabilities Capabilities { get; }

    /// <summary>Submits one ordered batch without splitting or rewriting it.</summary>
    Task<GenerationOperationSnapshot<GenerationBatchResponse<AudioGenerationResponse>>>
        StartBatchAsync(
            GenerationBatchRequest<AudioGenerationRequest> request,
            CancellationToken cancellationToken = default);

    /// <summary>Polls one previously submitted batch using its latest ticket.</summary>
    Task<GenerationOperationSnapshot<GenerationBatchResponse<AudioGenerationResponse>>>
        PollBatchAsync(
            GenerationOperationTicket ticket,
            CancellationToken cancellationToken = default);
}
