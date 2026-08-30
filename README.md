# Crystal

Crystal is a provider-neutral C# library for text and multimodal model access,
image, audio, and video generation, tool execution, bounded Agents, and explicit
Agent Harness composition.

Text and multimodal Chat, Tool, Agent, and Harness APIs are independent. Existing
text interfaces remain unchanged and text-only.

## Design guarantees

- No built-in provider, authentication, transport, or model catalog.
- No built-in prompt or runtime-authored model text.
- No concrete tools.
- Ordered readable and opaque reasoning preservation.
- Explicit media ownership, MIME types, and typed modality capabilities.
- Independent target-output image, audio, and video generation clients.
- Explicit candidate, tool, approval, limit, and composition policies.
- Immutable public data contracts.

## Project status

Crystal targets net10.0 and has no compatibility baseline yet. The current
repository implements the text foundation and the initial non-streaming
multimodal and immediate-generation scope described in ROADMAP.md. A test project
has not yet been authorized; the executable quality gate is:

~~~bash
dotnet build Crystal.sln
~~~

## Using Crystal

Crystal does not connect to a model provider by itself. An external adapter
implements only the text, multimodal, or generation client interfaces it can
honor. Provider selection, model identifiers, credentials, temperature, Top-P,
and other wire options stay in that adapter.

During local development, a consumer can reference the project directly:

~~~xml
<ItemGroup>
  <ProjectReference Include="../Crystal/Crystal.csproj" />
</ItemGroup>
~~~

### Chat

A system prompt is an ordinary caller-authored system message. Crystal preserves
it exactly and never adds another prompt:

~~~csharp
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Crystal.Chat;

namespace Example;

public static class ChatExample
{
    public static async Task<string> AskAsync(
        IChatClient client,
        string systemPrompt,
        string question,
        CancellationToken cancellationToken)
    {
        var request = new ChatRequest(
        [
            new ChatMessage(ChatRole.System, systemPrompt),
            new ChatMessage(ChatRole.User, question)
        ]);

        var response = await client.CompleteAsync(
            request,
            cancellationToken);
        var candidate = response.Candidates[0];

        return string.Concat(
            candidate.Items
                .OfType<ChatMessage>()
                .Where(static message =>
                    message.Role == ChatRole.Assistant)
                .Select(static message => message.Text));
    }
}
~~~

The example explicitly selects candidate zero. Applications that request
multiple candidates must own their selection policy.

### Multimodal Chat

Multimodal Chat is separate from IChatClient. Content blocks are strongly typed,
ordered, and carry explicit media ownership and MIME information:

~~~csharp
using System;
using System.Threading;
using System.Threading.Tasks;

using Crystal.Media;
using Crystal.Multimodal;
using Crystal.Multimodal.Chat;

namespace Example;

public static class MultimodalChatExample
{
    public static Task<MultimodalChatResponse> AskAboutImageAsync(
        IMultimodalChatClient client,
        ReadOnlyMemory<byte> pngBytes,
        string question,
        CancellationToken cancellationToken)
    {
        var image = new ImageMedia(
            new InlineMediaSource(pngBytes),
            new MediaMimeType("image/png"));
        var request = new MultimodalChatRequest(
        [
            new MultimodalMessage(
                MultimodalChatRole.User,
                [new TextContent(question), new ImageContent(image)])
        ]);

        return client.CompleteAsync(request, cancellationToken);
    }
}
~~~

Use UriMediaSource only when the adapter advertises URI support; Crystal does not
download it. URI and replayable sources can report ExpiresAt without Crystal
refreshing them. ReplayableStreamMediaSource opens a fresh caller-owned stream
for each attempt, and the consumer disposes each returned stream.

### Image, audio, and video generation

Generation clients are separated by target output. All can accept the same
closed typed input family when their capability profile advertises it. This
video example supplies an image first frame and an audio reference:

~~~csharp
using System;
using System.Threading;
using System.Threading.Tasks;

using Crystal.Generation;
using Crystal.Generation.Video;
using Crystal.Media;

namespace Example;

public static class VideoGenerationExample
{
    public static Task<VideoGenerationResponse> GenerateAsync(
        IVideoGenerationClient client,
        ImageMedia firstFrame,
        AudioMedia audioReference,
        string instruction,
        CancellationToken cancellationToken)
    {
        var request = new VideoGenerationRequest(
        [
            new GenerationTextInput(instruction),
            new GenerationImageInput(
                firstFrame,
                GenerationInputPurpose.FirstFrame),
            new GenerationAudioInput(
                audioReference,
                GenerationInputPurpose.Reference)
        ],
        new VideoGenerationRequirements(
            aspectRatio: new AspectRatio(16, 9),
            duration: TimeSpan.FromSeconds(8),
            audio: VideoAudioRequirement.Required));

        return client.GenerateAsync(request, cancellationToken);
    }
}
~~~

Source and mask inputs express editing or transformation; there is no universal
edit mode. Output requirements are hard, so an adapter rejects combinations it
cannot honor. A VideoContent reports embedded-audio presence on VideoMedia; a
separate AudioContent remains a separate ordered output item. Voice identities,
pronunciation controls, music styles, and other provider-specific audio options
stay on adapter APIs.

### Completion and embeddings

~~~csharp
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Crystal.Completions;
using Crystal.Embeddings;

namespace Example;

public static class DirectModelExample
{
    public static async Task<string> CompleteAsync(
        ICompletionClient client,
        string prompt,
        CancellationToken cancellationToken)
    {
        var response = await client.CompleteAsync(
            new CompletionRequest(prompt),
            cancellationToken);

        return string.Concat(
            response.Candidates[0].Items
                .OfType<CompletionText>()
                .Select(static item => item.Text));
    }

    public static async Task<ReadOnlyMemory<float>> EmbedAsync(
        IEmbeddingClient client,
        string text,
        CancellationToken cancellationToken)
    {
        var response = await client.EmbedAsync(
            new EmbeddingRequest([text]),
            cancellationToken);

        return response.Vectors[0].Values;
    }
}
~~~

### Tools and Agent

Callers implement ITool, register tools in an immutable catalog, choose serial or
bounded-concurrent execution, and provide a candidate-selection policy:

~~~csharp
using System;
using System.Threading;
using System.Threading.Tasks;

using Crystal.Agents;
using Crystal.Chat;
using Crystal.Tools;

namespace Example;

public static class AgentExample
{
    private const int MaximumModelCalls = 8;
    private const int MaximumToolCalls = 8;

    public static Task<AgentRunResult> RunAsync(
        IChatClient client,
        ITool tool,
        string systemPrompt,
        string requestText,
        CancellationToken cancellationToken)
    {
        var executor = new ToolExecutor(
            new ToolCatalog([tool]),
            new ToolExecutionOptions(
                ToolExecutionMode.Serial,
                maximumConcurrency: 1));
        IAgent agent = new Agent(
            client,
            static (_, _) => ValueTask.FromResult(0),
            executor);

        return agent.RunAsync(
            new AgentRunRequest(
                Guid.NewGuid(),
                [
                    new ChatMessage(ChatRole.System, systemPrompt),
                    new ChatMessage(ChatRole.User, requestText)
                ],
                new AgentRunLimits(
                    MaximumModelCalls,
                    MaximumToolCalls,
                    TimeSpan.FromMinutes(1))),
            cancellationToken);
    }
}
~~~

ITool receives exact raw model arguments in ToolCall.Arguments and returns exact
caller-owned text in ToolOutput. The Agent never repairs either value. Run usage
is available only when every attempted model call reports usage.

Multimodal tools and Agents use the independent IMultimodalTool,
IMultimodalToolExecutor, IMultimodalAgent, and MultimodalAgent contracts. They do
not inherit from or widen the text Tool and Agent families. IMultimodalAgent
exposes its fixed model input and output capabilities directly.

### Harness

Register Agents under case-sensitive names, create a bounded session, and invoke
each Agent explicitly. Parent invocation identifiers express ancestry; Harness
does not choose routes:

~~~csharp
using System;
using System.Threading;
using System.Threading.Tasks;

using Crystal.Agents;
using Crystal.Chat;
using Crystal.Harness;

namespace Example;

public static class HarnessExample
{
    private const int MaximumDepth = 2;
    private const int MaximumModelCalls = 20;
    private const int MaximumToolCalls = 20;
    private static readonly TimeSpan MaximumDuration =
        TimeSpan.FromMinutes(2);

    public static Task<AgentInvocationResult> InvokeAsync(
        IAgent agent,
        AgentRunLimits perAgentLimits,
        string requestText,
        CancellationToken cancellationToken)
    {
        var name = new AgentName("assistant");
        var harness = new AgentHarness(
        [
            new AgentRegistration(name, agent)
        ]);
        var session = harness.CreateSession(
            Guid.NewGuid(),
            new HarnessLimits(
                MaximumDepth,
                MaximumModelCalls,
                MaximumToolCalls,
                MaximumDuration),
            cancellationToken);

        return session.InvokeAsync(
            new AgentInvocationRequest(
                Guid.NewGuid(),
                name,
                [new ChatMessage(ChatRole.User, requestText)],
                perAgentLimits),
            cancellationToken);
    }
}
~~~

To invoke a child, pass the completed or registered parent invocation identifier
through AgentInvocationRequest.ParentInvocationId.

Text streaming adapters use candidate and item indexes to preserve interleaving.
Reasoning text deltas also use TextSegmentIndex so multiple readable segments can
be reconstructed without treating transport chunks as semantic boundaries.

## Architecture

Start with:

- BUSINESS.md for product scope;
- ARCHITECTURE.md for ownership and runtime semantics;
- COMPATIBILITY.md for adapter requirements;
- STANDARDS.md for engineering rules; and
- ROADMAP.md for delivery status and deferred media lifecycles.

## Provider adapters

A provider package implements only the capabilities it can preserve:

- IEmbeddingClient for text embeddings;
- ICompletionClient and optionally IStreamingCompletionClient;
- IChatClient and optionally IStreamingChatClient;
- IMultimodalChatClient with explicit input and output capabilities;
- IImageGenerationClient for immediate image generation;
- IAudioGenerationClient for immediate audio generation; and
- IVideoGenerationClient for immediate video generation.

Provider configuration, model identifiers, wire options, DTOs, and exceptions
stay in that external package.

## Prompt provenance

Crystal never creates natural-language content for a model. During an Agent run,
the transcript contains only caller input, selected model output, and exact
caller-owned tool output. Limits and errors stop the run or throw; they do not
become hidden messages.

## Deferred media lifecycles

The current media scope is non-streaming multimodal Chat and immediate
single-request image, audio, and video generation. Batch submission,
generated-media previews or chunks, resumable long-running remote operations,
explicit remote cancellation, and stateful realtime audio/video sessions require
separate future contracts. They will not
be added as modes on the immediate generation clients or as a generic attachment
bag.
