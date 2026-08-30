namespace Crystal.Generation.Images;

/// <summary>Defines provider-neutral immediate image generation.</summary>
public interface IImageGenerationClient
{
    /// <summary>
    /// Gets portable support. Outputs must include the image modality.
    /// </summary>
    GenerationCapabilities Capabilities { get; }

    /// <summary>Generates image candidates from ordered typed inputs.</summary>
    /// <param name="request">The exact generation request.</param>
    /// <param name="cancellationToken">A token that cancels local waiting.</param>
    /// <returns>The complete immediate response.</returns>
    Task<ImageGenerationResponse> GenerateAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default);
}
