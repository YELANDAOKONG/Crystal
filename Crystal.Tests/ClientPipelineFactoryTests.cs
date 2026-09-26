using System.Runtime.CompilerServices;

using Crystal;
using Crystal.Chat;
using Crystal.ClientPipelines;
using Crystal.Generation;
using Crystal.Generation.Images;
using Crystal.Media;
using Crystal.Multimodal;
using Crystal.Multimodal.Chat;
using Crystal.Pipelines;

namespace Crystal.Tests;

public sealed class ClientPipelineFactoryTests
{
    [Fact]
    public async Task ChatWrapperPreservesStreamingAndExactProtocolValues()
    {
        var response = new ChatResponse(
            [new ChatCandidate([new ChatMessage(ChatRole.Assistant, "answer")], FinishReason.Stop)]);
        var streamEvent = new ChatTextDelta(0, 0, ChatRole.Assistant, "answer");
        var source = new RecordingStreamingChatClient(response, streamEvent);
        var transitions = new List<string>();
        AsyncMiddleware<ChatRequest, ChatResponse> completeMiddleware = next =>
            async (request, token) =>
            {
                transitions.Add("complete before");
                var result = await next(request, token).ConfigureAwait(false);
                transitions.Add("complete after");
                return result;
            };
        StreamingMiddleware<ChatRequest, ChatStreamEvent> streamMiddleware = next =>
            (request, token) => Observe(next(request, token));

        async IAsyncEnumerable<ChatStreamEvent> Observe(
            IAsyncEnumerable<ChatStreamEvent> events)
        {
            await foreach (var item in events.ConfigureAwait(false))
            {
                transitions.Add("stream event");
                yield return item;
            }
        }

        var wrapped = ClientPipelineFactory.ForChat(
            source,
            [completeMiddleware],
            [streamMiddleware]);
        var streaming = Assert.IsAssignableFrom<IStreamingChatClient>(wrapped);
        var request = new ChatRequest([new ChatMessage(ChatRole.User, "question")]);
        using var cancellation = new CancellationTokenSource();

        var actualResponse = await wrapped.CompleteAsync(request, cancellation.Token);
        var actualEvents = new List<ChatStreamEvent>();
        await foreach (var item in streaming.StreamAsync(request, cancellation.Token))
        {
            actualEvents.Add(item);
        }

        Assert.Same(response, actualResponse);
        Assert.Same(streamEvent, Assert.Single(actualEvents));
        Assert.Same(request, source.CompleteRequest);
        Assert.Same(request, source.StreamRequest);
        Assert.Equal(cancellation.Token, source.CompleteToken);
        Assert.Equal(cancellation.Token, source.StreamToken);
        Assert.Equal(
            new[] { "complete before", "complete after", "stream event" },
            transitions);
    }

    [Fact]
    public void NonStreamingChatWrapperRejectsStreamingMiddleware()
    {
        var client = new RecordingChatClient();
        StreamingMiddleware<ChatRequest, ChatStreamEvent> middleware = next => next;

        var failure = Assert.Throws<ArgumentException>(() =>
            ClientPipelineFactory.ForChat(client, [], [middleware]));

        Assert.Equal("streamingMiddleware", failure.ParamName);
        Assert.IsNotAssignableFrom<IStreamingChatClient>(
            ClientPipelineFactory.ForChat(client, []));
    }

    [Fact]
    public async Task MultimodalAndGenerationWrappersPreserveCapabilities()
    {
        var text = new MultimodalContentCapability(ContentModality.Text);
        var image = new MultimodalContentCapability(
            ContentModality.Image,
            [MediaSourceKind.Inline]);
        var chatCapabilities = new MultimodalChatCapabilities([text], [text]);
        var chatResponse = new MultimodalChatResponse(
            [new MultimodalChatCandidate([], FinishReason.Stop)]);
        var chatSource = new RecordingMultimodalChatClient(chatCapabilities, chatResponse);
        var chat = ClientPipelineFactory.ForMultimodalChat(chatSource, []);
        var chatRequest = new MultimodalChatRequest([]);

        Assert.Same(chatCapabilities, chat.Capabilities);
        Assert.Same(chatResponse, await chat.CompleteAsync(chatRequest));
        Assert.Same(chatRequest, chatSource.Request);

        var imageCapabilities = new GenerationCapabilities([], [image]);
        var imageResponse = new ImageGenerationResponse([]);
        var imageSource = new RecordingImageGenerationClient(
            imageCapabilities,
            imageResponse);
        var generator = ClientPipelineFactory.ForImageGeneration(imageSource, []);
        var generationRequest = new ImageGenerationRequest([]);

        Assert.Same(imageCapabilities, generator.Capabilities);
        Assert.Same(imageResponse, await generator.GenerateAsync(generationRequest));
        Assert.Same(generationRequest, imageSource.Request);
    }

    [Fact]
    public async Task MultimodalWrapperPreservesStreamingAndCapabilityProfile()
    {
        var text = new MultimodalContentCapability(ContentModality.Text);
        var capabilities = new MultimodalChatCapabilities([text], [text]);
        var response = new MultimodalChatResponse(
            [new MultimodalChatCandidate([], FinishReason.Stop)]);
        var streamEvent = new MultimodalChatCandidateCompleted(0, FinishReason.Stop);
        var source = new RecordingStreamingMultimodalChatClient(
            capabilities,
            response,
            streamEvent);
        var wrapped = ClientPipelineFactory.ForMultimodalChat(source, []);
        var streaming = Assert.IsAssignableFrom<IStreamingMultimodalChatClient>(wrapped);
        var request = new MultimodalChatRequest([]);

        Assert.Same(capabilities, wrapped.Capabilities);
        Assert.Same(response, await wrapped.CompleteAsync(request));
        var events = new List<MultimodalChatStreamEvent>();
        await foreach (var item in streaming.StreamAsync(request))
        {
            events.Add(item);
        }

        Assert.Same(streamEvent, Assert.Single(events));
        Assert.Same(request, source.CompleteRequest);
        Assert.Same(request, source.StreamRequest);
    }

    private sealed class RecordingChatClient : IChatClient
    {
        public Task<ChatResponse> CompleteAsync(
            ChatRequest request,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    private sealed class RecordingStreamingChatClient(
        ChatResponse response,
        ChatStreamEvent streamEvent) : IStreamingChatClient
    {
        public ChatRequest? CompleteRequest { get; private set; }

        public ChatRequest? StreamRequest { get; private set; }

        public CancellationToken CompleteToken { get; private set; }

        public CancellationToken StreamToken { get; private set; }

        public Task<ChatResponse> CompleteAsync(
            ChatRequest request,
            CancellationToken cancellationToken = default)
        {
            CompleteRequest = request;
            CompleteToken = cancellationToken;
            return Task.FromResult(response);
        }

        public async IAsyncEnumerable<ChatStreamEvent> StreamAsync(
            ChatRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            StreamRequest = request;
            StreamToken = cancellationToken;
            yield return streamEvent;
            await Task.Yield();
        }
    }

    private sealed class RecordingMultimodalChatClient(
        MultimodalChatCapabilities capabilities,
        MultimodalChatResponse response) : IMultimodalChatClient
    {
        public MultimodalChatCapabilities Capabilities => capabilities;

        public MultimodalChatRequest? Request { get; private set; }

        public Task<MultimodalChatResponse> CompleteAsync(
            MultimodalChatRequest request,
            CancellationToken cancellationToken = default)
        {
            Request = request;
            return Task.FromResult(response);
        }
    }

    private sealed class RecordingImageGenerationClient(
        GenerationCapabilities capabilities,
        ImageGenerationResponse response) : IImageGenerationClient
    {
        public GenerationCapabilities Capabilities => capabilities;

        public ImageGenerationRequest? Request { get; private set; }

        public Task<ImageGenerationResponse> GenerateAsync(
            ImageGenerationRequest request,
            CancellationToken cancellationToken = default)
        {
            Request = request;
            return Task.FromResult(response);
        }
    }

    private sealed class RecordingStreamingMultimodalChatClient(
        MultimodalChatCapabilities capabilities,
        MultimodalChatResponse response,
        MultimodalChatStreamEvent streamEvent) : IStreamingMultimodalChatClient
    {
        public MultimodalChatCapabilities Capabilities => capabilities;

        public MultimodalChatRequest? CompleteRequest { get; private set; }

        public MultimodalChatRequest? StreamRequest { get; private set; }

        public Task<MultimodalChatResponse> CompleteAsync(
            MultimodalChatRequest request,
            CancellationToken cancellationToken = default)
        {
            CompleteRequest = request;
            return Task.FromResult(response);
        }

        public async IAsyncEnumerable<MultimodalChatStreamEvent> StreamAsync(
            MultimodalChatRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            StreamRequest = request;
            yield return streamEvent;
            await Task.Yield();
        }
    }
}
