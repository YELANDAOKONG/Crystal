using Crystal.Chat;
using Crystal.Completions;
using Crystal.Embeddings;
using Crystal.Generation.Audio;
using Crystal.Generation.Images;
using Crystal.Generation.Video;
using Crystal.Multimodal.Chat;
using Crystal.Pipelines;

namespace Crystal.Decorators;

/// <summary>Composes caller-owned middleware around provider-neutral clients.</summary>
public static class Clients
{
    /// <summary>Wraps text Chat while preserving optional streaming support.</summary>
    /// <param name="client">The configured text Chat client.</param>
    /// <param name="middleware">Ordered complete-response middleware.</param>
    /// <param name="streamingMiddleware">Optional ordered stream middleware.</param>
    /// <returns>A client with the same streaming capability as the input.</returns>
    public static IChatClient ForChat(
        IChatClient client,
        IEnumerable<AsyncMiddleware<ChatRequest, ChatResponse>> middleware,
        IEnumerable<StreamingMiddleware<ChatRequest, ChatStreamEvent>>? streamingMiddleware = null)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        ArgumentNullException.ThrowIfNull(middleware, nameof(middleware));

        var streamSteps = streamingMiddleware?.ToArray() ?? [];
        if (client is IStreamingChatClient streamingClient)
        {
            return new StreamingChatClientAdapter(streamingClient, middleware, streamSteps);
        }

        RequireNoStreamingMiddleware(streamSteps, nameof(streamingMiddleware));
        return new ChatClientAdapter(client, middleware);
    }

    /// <summary>Wraps text Completion while preserving optional streaming support.</summary>
    /// <param name="client">The configured Completion client.</param>
    /// <param name="middleware">Ordered complete-response middleware.</param>
    /// <param name="streamingMiddleware">Optional ordered stream middleware.</param>
    /// <returns>A client with the same streaming capability as the input.</returns>
    public static ICompletionClient ForCompletion(
        ICompletionClient client,
        IEnumerable<AsyncMiddleware<CompletionRequest, CompletionResponse>> middleware,
        IEnumerable<StreamingMiddleware<CompletionRequest, CompletionStreamEvent>>? streamingMiddleware = null)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        ArgumentNullException.ThrowIfNull(middleware, nameof(middleware));

        var streamSteps = streamingMiddleware?.ToArray() ?? [];
        if (client is IStreamingCompletionClient streamingClient)
        {
            return new StreamingCompletionClientAdapter(streamingClient, middleware, streamSteps);
        }

        RequireNoStreamingMiddleware(streamSteps, nameof(streamingMiddleware));
        return new CompletionClientAdapter(client, middleware);
    }

    /// <summary>Wraps multimodal Chat while preserving capabilities and optional streaming.</summary>
    /// <param name="client">The configured multimodal Chat client.</param>
    /// <param name="middleware">Ordered complete-response middleware.</param>
    /// <param name="streamingMiddleware">Optional ordered stream middleware.</param>
    /// <returns>A client with the same capability profile and streaming support.</returns>
    public static IMultimodalChatClient ForMultimodalChat(
        IMultimodalChatClient client,
        IEnumerable<AsyncMiddleware<MultimodalChatRequest, MultimodalChatResponse>> middleware,
        IEnumerable<StreamingMiddleware<MultimodalChatRequest, MultimodalChatStreamEvent>>? streamingMiddleware = null)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        ArgumentNullException.ThrowIfNull(middleware, nameof(middleware));

        var streamSteps = streamingMiddleware?.ToArray() ?? [];
        if (client is IStreamingMultimodalChatClient streamingClient)
        {
            return new StreamingMultimodalChatClientAdapter(
                streamingClient,
                middleware,
                streamSteps);
        }

        RequireNoStreamingMiddleware(streamSteps, nameof(streamingMiddleware));
        return new MultimodalChatClientAdapter(client, middleware);
    }

    /// <summary>Wraps text Embedding.</summary>
    /// <param name="client">The configured Embedding client.</param>
    /// <param name="middleware">Ordered middleware.</param>
    /// <returns>The wrapped Embedding client.</returns>
    public static IEmbeddingClient ForEmbedding(
        IEmbeddingClient client,
        IEnumerable<AsyncMiddleware<EmbeddingRequest, EmbeddingResponse>> middleware) =>
        new EmbeddingClientAdapter(client, middleware);

    /// <summary>Wraps immediate image generation without changing capabilities.</summary>
    /// <param name="client">The configured image-generation client.</param>
    /// <param name="middleware">Ordered middleware.</param>
    /// <returns>The wrapped image-generation client.</returns>
    public static IImageGenerationClient ForImageGeneration(
        IImageGenerationClient client,
        IEnumerable<AsyncMiddleware<ImageGenerationRequest, ImageGenerationResponse>> middleware) =>
        new ImageGenerationClientAdapter(client, middleware);

    /// <summary>Wraps immediate audio generation without changing capabilities.</summary>
    /// <param name="client">The configured audio-generation client.</param>
    /// <param name="middleware">Ordered middleware.</param>
    /// <returns>The wrapped audio-generation client.</returns>
    public static IAudioGenerationClient ForAudioGeneration(
        IAudioGenerationClient client,
        IEnumerable<AsyncMiddleware<AudioGenerationRequest, AudioGenerationResponse>> middleware) =>
        new AudioGenerationClientAdapter(client, middleware);

    /// <summary>Wraps immediate video generation without changing capabilities.</summary>
    /// <param name="client">The configured video-generation client.</param>
    /// <param name="middleware">Ordered middleware.</param>
    /// <returns>The wrapped video-generation client.</returns>
    public static IVideoGenerationClient ForVideoGeneration(
        IVideoGenerationClient client,
        IEnumerable<AsyncMiddleware<VideoGenerationRequest, VideoGenerationResponse>> middleware) =>
        new VideoGenerationClientAdapter(client, middleware);

    private static void RequireNoStreamingMiddleware<TRequest, TEvent>(
        IReadOnlyList<StreamingMiddleware<TRequest, TEvent>> middleware,
        string parameterName)
    {
        if (middleware.Count > 0)
        {
            throw new ArgumentException(
                "Streaming middleware requires a streaming client.",
                parameterName);
        }
    }
}
