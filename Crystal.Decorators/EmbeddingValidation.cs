using Crystal.Embeddings;
using Crystal.Multimodal.Embeddings;
using Crystal.Pipelines;

namespace Crystal.Decorators;

/// <summary>Provides opt-in cardinality checks for embedding clients.</summary>
public static class EmbeddingValidation
{
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
}
