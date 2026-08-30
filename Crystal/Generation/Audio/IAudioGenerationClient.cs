namespace Crystal.Generation.Audio;

/// <summary>Defines provider-neutral immediate audio generation.</summary>
public interface IAudioGenerationClient
{
    /// <summary>
    /// Gets portable support. Outputs must include the audio modality.
    /// </summary>
    GenerationCapabilities Capabilities { get; }

    /// <summary>Generates audio candidates from ordered typed inputs.</summary>
    /// <param name="request">The exact generation request.</param>
    /// <param name="cancellationToken">A token that cancels local waiting.</param>
    /// <returns>The complete immediate response.</returns>
    Task<AudioGenerationResponse> GenerateAsync(
        AudioGenerationRequest request,
        CancellationToken cancellationToken = default);
}
