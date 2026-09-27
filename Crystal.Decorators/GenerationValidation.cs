using System.Runtime.CompilerServices;

using Crystal.Generation;
using Crystal.Media;
using Crystal.Pipelines;

namespace Crystal.Decorators;

/// <summary>Provides opt-in preflight for declared generation input shapes.</summary>
public static class GenerationValidation
{
    /// <summary>
    /// Rejects generation inputs with an undeclared modality, purpose, or media
    /// source kind before an asynchronous client operation.
    /// </summary>
    /// <typeparam name="TRequest">The exact client request type.</typeparam>
    /// <typeparam name="TResponse">The exact client response type.</typeparam>
    /// <param name="capabilities">The configured client's declared support.</param>
    /// <param name="selectInputs">Selects the request's generation inputs.</param>
    /// <returns>Middleware that forwards valid requests unchanged.</returns>
    public static AsyncMiddleware<TRequest, TResponse> RequireDeclaredInputShapes<
        TRequest, TResponse>(
            GenerationCapabilities capabilities,
            Func<TRequest, IEnumerable<GenerationInput>> selectInputs)
    {
        ArgumentNullException.ThrowIfNull(capabilities);
        ArgumentNullException.ThrowIfNull(selectInputs);

        return next =>
        {
            ArgumentNullException.ThrowIfNull(next);

            return (request, cancellationToken) =>
            {
                ArgumentNullException.ThrowIfNull(request);
                cancellationToken.ThrowIfCancellationRequested();
                ValidateInputs(capabilities, selectInputs(request), nameof(request));
                return next(request, cancellationToken);
            };
        };
    }

    /// <summary>
    /// Rejects generation inputs with an undeclared modality, purpose, or media
    /// source kind before starting a streamed client operation.
    /// </summary>
    /// <typeparam name="TRequest">The exact client request type.</typeparam>
    /// <typeparam name="TEvent">The exact client stream-event type.</typeparam>
    /// <param name="capabilities">The configured client's declared support.</param>
    /// <param name="selectInputs">Selects the request's generation inputs.</param>
    /// <returns>Middleware that forwards valid events unchanged.</returns>
    public static StreamingMiddleware<TRequest, TEvent>
        RequireDeclaredStreamInputShapes<TRequest, TEvent>(
            GenerationCapabilities capabilities,
            Func<TRequest, IEnumerable<GenerationInput>> selectInputs)
    {
        ArgumentNullException.ThrowIfNull(capabilities);
        ArgumentNullException.ThrowIfNull(selectInputs);

        return next =>
        {
            ArgumentNullException.ThrowIfNull(next);

            return (request, cancellationToken) => ValidateStreamAsync(
                next, request, cancellationToken, capabilities, selectInputs);
        };
    }

    private static async IAsyncEnumerable<TEvent> ValidateStreamAsync<
        TRequest, TEvent>(
            StreamingOperation<TRequest, TEvent> next,
            TRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken,
            GenerationCapabilities capabilities,
            Func<TRequest, IEnumerable<GenerationInput>> selectInputs)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        ValidateInputs(capabilities, selectInputs(request), nameof(request));

        var source = next(request, cancellationToken)
            ?? throw new InvalidOperationException(
                "The generation client returned no stream.");

        await foreach (var streamEvent in source
            .WithCancellation(cancellationToken)
            .ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return streamEvent;
        }

        cancellationToken.ThrowIfCancellationRequested();
    }

    private static void ValidateInputs(
        GenerationCapabilities capabilities,
        IEnumerable<GenerationInput>? inputs,
        string parameterName)
    {
        if (inputs is null)
        {
            throw new InvalidOperationException(
                "The generation input selector returned no collection.");
        }

        foreach (var input in inputs)
        {
            if (input is null)
            {
                throw new ArgumentException(
                    "The generation request contains no input value.",
                    parameterName);
            }

            MediaSourceKind? sourceKind = input switch
            {
                GenerationTextInput => null,
                GenerationImageInput image => image.Image.Source.Kind,
                GenerationAudioInput audio => audio.Audio.Source.Kind,
                GenerationVideoInput video => video.Video.Source.Kind,
                _ => throw new InvalidOperationException(
                    "The generation request contains an unknown input type.")
            };

            if (!capabilities.Inputs.Any(capability =>
                capability.Purpose == input.Purpose
                && capability.Content.Modality == input.Modality
                && (sourceKind is null
                    || capability.Content.SourceKinds.Contains(sourceKind))))
            {
                throw new ArgumentException(
                    "The generation request contains an undeclared input shape.",
                    parameterName);
            }
        }
    }
}
