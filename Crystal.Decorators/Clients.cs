using Crystal.Chat;
using Crystal.Completions;
using Crystal.Decorators.Chat;
using Crystal.Decorators.Completions;
using Crystal.Decorators.Embeddings;
using Crystal.Decorators.Generation;
using Crystal.Decorators.Realtime;
using Crystal.Embeddings;
using Crystal.Generation.Audio;
using Crystal.Generation.Batches;
using Crystal.Generation.Images;
using Crystal.Generation.Operations;
using Crystal.Generation.Streaming;
using Crystal.Generation.Video;
using Crystal.Multimodal.Chat;
using Crystal.Multimodal.Embeddings;
using Crystal.Pipelines;
using Crystal.Realtime;

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

    /// <summary>Wraps multimodal Embedding while preserving its capabilities.</summary>
    /// <param name="client">The configured multimodal Embedding client.</param>
    /// <param name="middleware">Ordered caller-owned middleware.</param>
    /// <returns>The wrapped client with the same capability profile.</returns>
    public static IMultimodalEmbeddingClient ForMultimodalEmbedding(
        IMultimodalEmbeddingClient client,
        IEnumerable<AsyncMiddleware<MultimodalEmbeddingRequest,
            MultimodalEmbeddingResponse>> middleware) =>
        new MultimodalEmbeddingClientAdapter(client, middleware);

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

    /// <summary>Wraps image-generation streaming without changing capabilities.</summary>
    /// <param name="client">The configured image-generation stream client.</param>
    /// <param name="middleware">Ordered stream middleware.</param>
    /// <returns>The wrapped image-generation stream client.</returns>
    public static IStreamingImageGenerationClient ForStreamingImageGeneration(
        IStreamingImageGenerationClient client,
        IEnumerable<StreamingMiddleware<ImageGenerationRequest,
            GenerationStreamEvent<ImageGenerationResponse>>> middleware) =>
        new StreamingImageGenerationClientAdapter(client, middleware);

    /// <summary>Wraps audio-generation streaming without changing capabilities.</summary>
    /// <param name="client">The configured audio-generation stream client.</param>
    /// <param name="middleware">Ordered stream middleware.</param>
    /// <returns>The wrapped audio-generation stream client.</returns>
    public static IStreamingAudioGenerationClient ForStreamingAudioGeneration(
        IStreamingAudioGenerationClient client,
        IEnumerable<StreamingMiddleware<AudioGenerationRequest,
            GenerationStreamEvent<AudioGenerationResponse>>> middleware) =>
        new StreamingAudioGenerationClientAdapter(client, middleware);

    /// <summary>Wraps video-generation streaming without changing capabilities.</summary>
    /// <param name="client">The configured video-generation stream client.</param>
    /// <param name="middleware">Ordered stream middleware.</param>
    /// <returns>The wrapped video-generation stream client.</returns>
    public static IStreamingVideoGenerationClient ForStreamingVideoGeneration(
        IStreamingVideoGenerationClient client,
        IEnumerable<StreamingMiddleware<VideoGenerationRequest,
            GenerationStreamEvent<VideoGenerationResponse>>> middleware) =>
        new StreamingVideoGenerationClientAdapter(client, middleware);

    /// <summary>Wraps image-generation operation start and poll separately.</summary>
    /// <param name="client">The configured image-generation operation client.</param>
    /// <param name="startMiddleware">Ordered submission middleware.</param>
    /// <param name="pollMiddleware">Ordered polling middleware.</param>
    /// <returns>The wrapped operation client.</returns>
    public static IImageGenerationOperationClient ForImageGenerationOperation(
        IImageGenerationOperationClient client,
        IEnumerable<AsyncMiddleware<ImageGenerationRequest,
            GenerationOperationSnapshot<ImageGenerationResponse>>> startMiddleware,
        IEnumerable<AsyncMiddleware<GenerationOperationTicket,
            GenerationOperationSnapshot<ImageGenerationResponse>>> pollMiddleware) =>
        new ImageGenerationOperationClientAdapter(
            client, startMiddleware, pollMiddleware);

    /// <summary>Wraps audio-generation operation start and poll separately.</summary>
    /// <param name="client">The configured audio-generation operation client.</param>
    /// <param name="startMiddleware">Ordered submission middleware.</param>
    /// <param name="pollMiddleware">Ordered polling middleware.</param>
    /// <returns>The wrapped operation client.</returns>
    public static IAudioGenerationOperationClient ForAudioGenerationOperation(
        IAudioGenerationOperationClient client,
        IEnumerable<AsyncMiddleware<AudioGenerationRequest,
            GenerationOperationSnapshot<AudioGenerationResponse>>> startMiddleware,
        IEnumerable<AsyncMiddleware<GenerationOperationTicket,
            GenerationOperationSnapshot<AudioGenerationResponse>>> pollMiddleware) =>
        new AudioGenerationOperationClientAdapter(
            client, startMiddleware, pollMiddleware);

    /// <summary>Wraps video-generation operation start and poll separately.</summary>
    /// <param name="client">The configured video-generation operation client.</param>
    /// <param name="startMiddleware">Ordered submission middleware.</param>
    /// <param name="pollMiddleware">Ordered polling middleware.</param>
    /// <returns>The wrapped operation client.</returns>
    public static IVideoGenerationOperationClient ForVideoGenerationOperation(
        IVideoGenerationOperationClient client,
        IEnumerable<AsyncMiddleware<VideoGenerationRequest,
            GenerationOperationSnapshot<VideoGenerationResponse>>> startMiddleware,
        IEnumerable<AsyncMiddleware<GenerationOperationTicket,
            GenerationOperationSnapshot<VideoGenerationResponse>>> pollMiddleware) =>
        new VideoGenerationOperationClientAdapter(
            client, startMiddleware, pollMiddleware);

    /// <summary>Wraps image-generation batch submission and polling separately.</summary>
    /// <param name="client">The configured image-generation batch client.</param>
    /// <param name="startMiddleware">Ordered batch submission middleware.</param>
    /// <param name="pollMiddleware">Ordered batch polling middleware.</param>
    /// <returns>The wrapped batch client.</returns>
    public static IImageGenerationBatchClient ForImageGenerationBatch(
        IImageGenerationBatchClient client,
        IEnumerable<AsyncMiddleware<GenerationBatchRequest<ImageGenerationRequest>,
            GenerationOperationSnapshot<GenerationBatchResponse<ImageGenerationResponse>>>>
            startMiddleware,
        IEnumerable<AsyncMiddleware<GenerationOperationTicket,
            GenerationOperationSnapshot<GenerationBatchResponse<ImageGenerationResponse>>>>
            pollMiddleware) =>
        new ImageGenerationBatchClientAdapter(
            client, startMiddleware, pollMiddleware);

    /// <summary>Wraps audio-generation batch submission and polling separately.</summary>
    /// <param name="client">The configured audio-generation batch client.</param>
    /// <param name="startMiddleware">Ordered batch submission middleware.</param>
    /// <param name="pollMiddleware">Ordered batch polling middleware.</param>
    /// <returns>The wrapped batch client.</returns>
    public static IAudioGenerationBatchClient ForAudioGenerationBatch(
        IAudioGenerationBatchClient client,
        IEnumerable<AsyncMiddleware<GenerationBatchRequest<AudioGenerationRequest>,
            GenerationOperationSnapshot<GenerationBatchResponse<AudioGenerationResponse>>>>
            startMiddleware,
        IEnumerable<AsyncMiddleware<GenerationOperationTicket,
            GenerationOperationSnapshot<GenerationBatchResponse<AudioGenerationResponse>>>>
            pollMiddleware) =>
        new AudioGenerationBatchClientAdapter(
            client, startMiddleware, pollMiddleware);

    /// <summary>Wraps video-generation batch submission and polling separately.</summary>
    /// <param name="client">The configured video-generation batch client.</param>
    /// <param name="startMiddleware">Ordered batch submission middleware.</param>
    /// <param name="pollMiddleware">Ordered batch polling middleware.</param>
    /// <returns>The wrapped batch client.</returns>
    public static IVideoGenerationBatchClient ForVideoGenerationBatch(
        IVideoGenerationBatchClient client,
        IEnumerable<AsyncMiddleware<GenerationBatchRequest<VideoGenerationRequest>,
            GenerationOperationSnapshot<GenerationBatchResponse<VideoGenerationResponse>>>>
            startMiddleware,
        IEnumerable<AsyncMiddleware<GenerationOperationTicket,
            GenerationOperationSnapshot<GenerationBatchResponse<VideoGenerationResponse>>>>
            pollMiddleware) =>
        new VideoGenerationBatchClientAdapter(
            client, startMiddleware, pollMiddleware);

    /// <summary>Wraps only live-session opening and preserves capabilities.</summary>
    /// <param name="client">The configured realtime media client.</param>
    /// <param name="middleware">Ordered session-opening middleware.</param>
    /// <returns>The wrapped realtime media client.</returns>
    public static IRealtimeMediaClient ForRealtimeMedia(
        IRealtimeMediaClient client,
        IEnumerable<AsyncMiddleware<RealtimeSessionRequest,
            IRealtimeMediaSession>> middleware) =>
        new RealtimeMediaClientAdapter(client, middleware);

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
