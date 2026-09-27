using Crystal.Embeddings;
using Crystal.Media;
using Crystal.Multimodal;
using Crystal.Multimodal.Embeddings;
using Crystal.Pipelines;

namespace Crystal.Decorators;

/// <summary>Provides opt-in cardinality checks for embedding clients.</summary>
public static class EmbeddingValidation
{
    /// <summary>Requires every multimodal input block to have a declared shape.</summary>
    /// <param name="capabilities">The client's individual input shapes.</param>
    /// <returns>Middleware that rejects undeclared modalities or source kinds.</returns>
    public static AsyncMiddleware<MultimodalEmbeddingRequest,
        MultimodalEmbeddingResponse> RequireDeclaredInputShapes(
        MultimodalEmbeddingCapabilities capabilities)
    {
        ArgumentNullException.ThrowIfNull(capabilities, nameof(capabilities));

        return next =>
        {
            ArgumentNullException.ThrowIfNull(next, nameof(next));

            return (request, cancellationToken) =>
            {
                ArgumentNullException.ThrowIfNull(request, nameof(request));
                cancellationToken.ThrowIfCancellationRequested();

                foreach (var input in request.Inputs)
                {
                    foreach (var content in input.Content)
                    {
                        if (!IsDeclared(capabilities, content))
                        {
                            throw new ArgumentException(
                                "The multimodal embedding request contains an undeclared input shape.",
                                nameof(request));
                        }
                    }
                }

                return next(request, cancellationToken);
            };
        };
    }

    /// <summary>Requires one text embedding vector per request input.</summary>
    /// <returns>Middleware that rejects a response with the wrong vector count.</returns>
    public static AsyncMiddleware<EmbeddingRequest, EmbeddingResponse>
        RequireTextCardinality() =>
        next =>
        {
            ArgumentNullException.ThrowIfNull(next, nameof(next));

            return async (request, cancellationToken) =>
            {
                ArgumentNullException.ThrowIfNull(request, nameof(request));

                var response = await next(request, cancellationToken)
                    .ConfigureAwait(false);

                if (response is null)
                {
                    throw new InvalidOperationException(
                        "The embedding client returned no response.");
                }

                if (response.Vectors.Count != request.Inputs.Count)
                {
                    throw new InvalidOperationException(
                        "The embedding client returned an unexpected vector count.");
                }

                return response;
            };
        };

    /// <summary>Requires one multimodal embedding vector per request input.</summary>
    /// <returns>Middleware that rejects a response with the wrong vector count.</returns>
    public static AsyncMiddleware<MultimodalEmbeddingRequest,
        MultimodalEmbeddingResponse> RequireMultimodalCardinality() =>
        next =>
        {
            ArgumentNullException.ThrowIfNull(next, nameof(next));

            return async (request, cancellationToken) =>
            {
                ArgumentNullException.ThrowIfNull(request, nameof(request));

                var response = await next(request, cancellationToken)
                    .ConfigureAwait(false);

                if (response is null)
                {
                    throw new InvalidOperationException(
                        "The multimodal embedding client returned no response.");
                }

                if (response.Vectors.Count != request.Inputs.Count)
                {
                    throw new InvalidOperationException(
                        "The multimodal embedding client returned an unexpected vector count.");
                }

                return response;
            };
        };

    private static bool IsDeclared(
        MultimodalEmbeddingCapabilities capabilities,
        MultimodalContent content)
    {
        MediaSourceKind? sourceKind = content switch
        {
            TextContent => null,
            ImageContent image => image.Image.Source.Kind,
            AudioContent audio => audio.Audio.Source.Kind,
            VideoContent video => video.Video.Source.Kind,
            _ => throw new InvalidOperationException(
                "The multimodal embedding request contains an unknown content type.")
        };

        return capabilities.Inputs.Any(input =>
            input.Modality == content.Modality
            && (sourceKind is null || input.SourceKinds.Contains(sourceKind)));
    }
}
