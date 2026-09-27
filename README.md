# Crystal

Crystal is a provider-neutral C# library for text and multimodal model access,
image, audio, and video generation, tool execution, Agents with optional limits,
explicit Agent Harness composition, generic asynchronous operation pipelines,
and typed workflow graphs.

Text and multimodal Chat, Tool, Agent, and Harness APIs are independent. Existing
text interfaces remain unchanged and text-only.

## Design guarantees

- No built-in provider, authentication, transport, or model catalog.
- No built-in prompt or runtime-authored model text.
- No concrete tools.
- Ordered readable and opaque reasoning preservation.
- Caller-authored hard JSON Schema requirements for final text output.
- Explicit media ownership, MIME types, and typed modality capabilities.
- Independent optional multimodal Embedding over ordered typed content inputs.
- Independent target-output image, audio, and video generation clients.
- Separate live duplex media session contracts with exact segments and tool
  correlation.
- Explicit candidate, tool, approval, limit, and composition policies.
- Immutable public data contracts.
- Caller-owned ordered middleware for typed operations and event streams.
- Opt-in caller-decided retries for complete asynchronous operations.
- Caller-defined typed workflow edges, conditions, bounded parallel steps,
  ordered routing events, and explicit step-limit stops.
- Caller-owned conversation storage and reconstruction across invocations.
- Exact text Agent forwarding of provider Chat stream events when available.

## Project status

Crystal targets net10.0 and has no compatibility baseline yet. The current
repository implements the text foundation, optional typed multimodal Chat
streaming, multimodal Embedding, independent immediate, streaming, remote
operation and batch generation, live media session contracts, an operation
pipeline library, and a typed workflow graph runtime. The current
quality checks are:

~~~bash
dotnet build Crystal.sln
dotnet test Crystal.Tests/Crystal.Tests.csproj
~~~

Crystal does not store or restore application sessions. Callers retain the
conversation and other state they need, then supply it in a later request.

Reasoning effort presets such as `ReasoningEffort.Low` are optional. Callers can
pass `new ReasoningEffort("very-high")` through `ReasoningOptions` when their
external adapter supports that value. Crystal forwards the value unchanged;
the adapter rejects unsupported values rather than substituting a preset.

### Operation pipelines

`Crystal.Pipelines` composes caller-owned middleware around any typed
asynchronous operation. The same pattern works for Chat, Completion, Embedding,
generation, tools, and application operations. A separate `StreamingPipeline`
preserves the `IAsyncEnumerable<T>` lifecycle.

~~~csharp
using Crystal.Chat;
using Crystal.Pipelines;

namespace Example;

public static class PipelineExample
{
    public static Task<ChatResponse> CompleteAsync(
        IChatClient client,
        ChatRequest request,
        CancellationToken cancellationToken)
    {
        AsyncMiddleware<ChatRequest, ChatResponse> middleware = next =>
            (exactRequest, token) => next(exactRequest, token);

        var pipeline = new AsyncPipeline<ChatRequest, ChatResponse>(
            client.CompleteAsync,
            [middleware]);

        return pipeline.InvokeAsync(request, cancellationToken);
    }
}
~~~

Middleware runs in declared order. Any request or response transformation is
explicit caller code and must satisfy the same content provenance rules as a
direct client call.

`PipelineDiagnostics.ObserveAsync` and `ObserveStreaming` can be added as
middleware. They report a stable caller-supplied operation name, start and
terminal status, and elapsed time. Stream timing spans enumeration through
disposal. Observations contain no request, response, event, or exception data;
the caller supplies an observer and any telemetry sink.

`Crystal.Decorators` provides `Clients` for wrapping the
current Chat, Completion, text and multimodal Embedding, multimodal Chat, and immediate-generation
clients. The wrapper keeps optional streaming support and capability profiles.
For example, `Clients.ForChat(client, middleware)` returns an
`IStreamingChatClient` at runtime when `client` supports streaming. Supplying
stream middleware to a non-streaming client is rejected during construction.
The same middleware delegates can express caller-owned policies: forward the
operation, reject it, or return an exact caller-owned result.

`EmbeddingValidation.RequireTextCardinality()` and
`EmbeddingValidation.RequireMultimodalCardinality()` are optional middleware
that reject a response with a vector count different from its request input
count. Adapters still own the correspondence and order of returned vectors.
`EmbeddingValidation.RequireDeclaredInputShapes(client.Capabilities)` can also
reject an undeclared multimodal Embedding modality or media source kind before
calling the client. It does not validate model-specific combinations or read
media.

### Workflow graphs

`Crystal.Workflows` runs caller-defined operations connected by type-matched
edges. A node can call an Agent, a tool, or ordinary application code without
the graph runtime knowing that implementation. Conditions route each exact
output value. Nodes reached in the same superstep receive one ordered batch;
concurrent node calls still route results in graph order.

~~~csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Crystal.Workflows;

namespace Example;

public static class WorkflowExample
{
    public static async Task<IReadOnlyList<int>> RunAsync(
        CancellationToken cancellationToken)
    {
        var start = new WorkflowNode<int, int>(
            "start",
            (inputs, _) => Task.FromResult(inputs));
        var doubled = new WorkflowNode<int, int>(
            "doubled",
            (inputs, _) => Task.FromResult<IReadOnlyList<int>>(
                inputs.Select(static value => value * 2).ToArray()));
        var workflow = new WorkflowBuilder<int, int>(start)
            .AddEdge(start, doubled)
            .Build(doubled);
        var result = await workflow.RunAsync(
            new WorkflowRunRequest<int>(Guid.NewGuid(), [1, 2]),
            cancellationToken);

        return result.Outputs;
    }
}
~~~

The result contains `[2, 4]`. `WorkflowLimits` can set a finite superstep
limit and bounded node concurrency. `StreamAsync` reports ordered node and route
metadata followed by the exact result. The graph owns no durable state or
human-interaction protocol.

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

An adapter may additionally implement IStreamingMultimodalChatClient. Streams
start messages explicitly, use stable indexes for message content, reasoning
parts, and tool-call content, and emit image, audio, and video blocks as complete
typed content events rather than byte chunks.

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

`JsonOutputRequirement` carries a caller-authored JSON Schema for normal final
text output. It is forwarded through Agent and Harness requests, while an
external adapter enforces the requirement or rejects unsupported combinations.
`IMultimodalEmbeddingClient` independently embeds ordered typed content blocks;
the text-only `IEmbeddingClient` stays unchanged.

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
Tool executors check cancellation at dispatch boundaries, including after a
caller policy returns; a started tool's side effects remain caller-owned.

AgentRunLimits accepts a finite maximum or `null` for each model-call,
tool-call, and duration budget. `AgentRunLimits.Unlimited` removes all three
configured bounds while caller cancellation still applies. Agent checks caller
cancellation between events and after client work, even if the client ignores
the token it received. A finite duration is also checked at those boundaries:
late client output is discarded and the run reports DurationLimitReached once
the client returns. A client that ignores cancellation can delay that report.

Multimodal tools and Agents use the independent IMultimodalTool,
IMultimodalToolExecutor, IMultimodalAgent, and MultimodalAgent contracts. They do
not inherit from or widen the text Tool and Agent families. IMultimodalAgent
exposes its fixed model input and output capabilities directly.

### Harness

Register Agents under case-sensitive names, create a session with chosen
budgets, and invoke each Agent explicitly. Parent invocation identifiers
express ancestry; Harness does not choose routes:

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

HarnessLimits likewise accepts `null` independently for depth, model calls,
tool calls, and duration. `HarnessLimits.Unlimited` removes all four shared
bounds; a finite shared maximum still narrows an unlimited Agent request.

Text streaming adapters use candidate and item indexes to preserve interleaving.
Reasoning text deltas also use TextSegmentIndex so multiple readable segments can
be reconstructed without treating transport chunks as semantic boundaries.

## Architecture

Start with:

- BUSINESS.md for product scope;
- ARCHITECTURE.md for ownership and runtime semantics;
- COMPATIBILITY.md for adapter requirements;
- STANDARDS.md for engineering rules.

## Provider adapters

A provider package implements only the capabilities it can preserve:

- IEmbeddingClient for text embeddings;
- ICompletionClient and optionally IStreamingCompletionClient;
- IChatClient and optionally IStreamingChatClient;
- IMultimodalChatClient and optionally IStreamingMultimodalChatClient, with
  explicit input and output capabilities;
- IImageGenerationClient for immediate image generation;
- IAudioGenerationClient for immediate audio generation;
- IVideoGenerationClient for immediate video generation; and
- IRealtimeMediaClient for live duplex text and typed media segments.

Optional `IImageGenerationOperationClient`,
`IAudioGenerationOperationClient`, and `IVideoGenerationOperationClient`
support remote operations. `StartAsync` and `PollAsync` each return a
`GenerationOperationSnapshot<TResponse>`. Callers retain its latest opaque
`GenerationOperationTicket`, choose when to poll, and provide their own storage
if the ticket must outlive the process. Only a Completed snapshot contains a
generation response. Local cancellation does not cancel an accepted remote job.

`Clients.ForImageGenerationOperation`, `ForAudioGenerationOperation`, and
`ForVideoGenerationOperation` wrap their independent client interfaces with
separate start and poll middleware; the caller still retains each ticket and
controls polling.

Optional `IImageGenerationBatchClient`, `IAudioGenerationBatchClient`, and
`IVideoGenerationBatchClient` submit an ordered batch as one remote operation.
When complete, the batch response has one terminal item at each input index,
including failed or canceled items. Crystal does not split a batch into
individual calls.

`Clients.ForImageGenerationBatch`, `ForAudioGenerationBatch`, and
`ForVideoGenerationBatch` likewise compose separate submission and polling
middleware without saving a ticket or batch.

`GenerationBatchValidation.RequireSubmittedCardinality<TRequest, TResponse>()`
can check an immediately completed submission in an `AsyncPipeline`. For a
completed poll, `RequirePolledCardinality<TResponse>(submittedCount)` checks
against the original count retained by the caller. Pending snapshots pass
through unchanged; Crystal stores no tickets or counts between calls.

Optional `IStreamingImageGenerationClient`,
`IStreamingAudioGenerationClient`, and `IStreamingVideoGenerationClient`
deliver complete provisional previews or copied encoded chunks with explicit
candidate, item, and revision indexes. A successful stream ends with one event
containing the authoritative complete response. Preview revisions can change;
the final response decides the result.

If a configured generation client implements both its immediate and streaming
interfaces, `Clients.ForImageGeneration`, `ForAudioGeneration`, or
`ForVideoGeneration` preserves both interfaces and accepts separate middleware
for complete responses and streams. Each interface retains its own declared
capabilities. Passing stream middleware for an immediate-only client fails at
construction.

`Clients.ForStreamingImageGeneration`, `ForStreamingAudioGeneration`, and
`ForStreamingVideoGeneration` compose stream middleware while retaining each
independent client interface and its declared capabilities.

`GenerationValidation.RequireDeclaredInputShapes` can preflight a generation
request's declared input modality, purpose, and media source kind in an
`AsyncPipeline`; `RequireDeclaredStreamInputShapes` does the same before a
stream starts. The caller supplies an input selector, so a batch can select
inputs from every submitted request. These checks do not read media or replace
adapter validation of model-specific combinations and output requirements.

`GenerationStreamValidation.RequireProtocol<TRequest, TResponse>()` can be
added to one of these wrappers or a `StreamingPipeline` to reject out-of-order
or incomplete provisional media and missing or repeated completion. It forwards
valid events unchanged and does not assemble bytes into the final response.

`RealtimeOutputValidation.ValidateAsync(session.ReceiveAsync(token), token)`
optionally checks each live output's segment order and completion boundary. It
forwards the same events and does not reconnect or store the session.

`RealtimeSessionValidation.RequireDeclaredCapabilities(client.Capabilities)`
can preflight individually advertised session settings in an `AsyncPipeline`
around `OpenAsync`; the adapter still checks model-specific combinations.

`Clients.ForRealtimeMedia` can compose that middleware while preserving the
declared capabilities and returning the exact live session.

Provider configuration, model identifiers, wire options, DTOs, and exceptions
stay in that external package.

## Prompt provenance

Crystal never creates natural-language content for a model. During an Agent run,
the transcript contains only caller input, selected model output, and exact
caller-owned tool output. Limits and errors stop the run or throw; they do not
become hidden messages.

## Media lifecycle boundaries

The current media scope includes non-streaming and optional typed streaming
multimodal Chat plus immediate, streaming, resumable-operation, and batch image,
audio, and video generation. The current live session contract handles duplex
text and typed media segments, tool correlation, and explicit boundaries.
Remote cancellation and connection resumption remain adapter and caller
concerns. These contracts are separate from immediate generation and do not use
a generic attachment bag.
