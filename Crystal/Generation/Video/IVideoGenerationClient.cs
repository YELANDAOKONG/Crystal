namespace Crystal.Generation.Video;

/// <summary>Defines provider-neutral immediate video generation.</summary>
public interface IVideoGenerationClient
{
    /// <summary>
    /// Gets portable support. Outputs must include the video modality.
    /// </summary>
    GenerationCapabilities Capabilities { get; }

    /// <summary>Generates video candidates from ordered typed inputs.</summary>
    /// <param name="request">The exact generation request.</param>
    /// <param name="cancellationToken">
    /// A token that cancels local waiting, not an already submitted remote job.
    /// </param>
    /// <returns>The complete immediate response.</returns>
    Task<VideoGenerationResponse> GenerateAsync(
        VideoGenerationRequest request,
        CancellationToken cancellationToken = default);
}
