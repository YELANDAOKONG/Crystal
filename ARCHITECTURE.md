# Crystal Architecture

## Status

This document is the authoritative architecture for the current text and
multimodal development line. There is no compatibility baseline yet, so public
names may change while the design documents and implementation change together.

## Dependency direction

Crystal currently ships six production assemblies. Four form a one-way
runtime dependency chain; Crystal.Pipelines is independent and
Crystal.Decorators depends only on the protocol and generic pipeline layers:

~~~text
Crystal.Harness
    ↓
Crystal.Agents
    ↓
Crystal.Tools
    ↓
Crystal

Crystal.Pipelines (no project references)

Crystal.Decorators → Crystal + Crystal.Pipelines
~~~

Crystal.Agents also references Crystal directly. Crystal.Harness also references
Crystal directly. No production project references a higher layer, and the
dependency graph contains no cycle.

External adapters depend only on Crystal unless they deliberately use a higher
runtime layer. External tools, policies, stores, applications, and orchestration
topologies depend on the lowest layer that contains the capability they need.
No Crystal assembly depends on external implementations.

## Assembly ownership

### Crystal

Owns cross-capability primitives, Reasoning, Embeddings, Completions, text Chat,
media sources and values, typed multimodal content and Chat, immediate image,
audio, and video generation, and all model-facing text and multimodal tool
protocol values. Keeping protocol values in Crystal lets provider adapters
represent complete traffic without depending on executable tool infrastructure.

### Crystal.Tools

Owns independent text and multimodal executable tool contracts, catalogs,
scheduling, policies, exception mapping, and dispatch. It references Crystal.

### Crystal.Agents

Owns independent text and multimodal Agent contracts, events, limits, results,
and runtime execution. It references Crystal and Crystal.Tools.

### Crystal.Harness

Owns independent text and multimodal Harness contracts, events, reservations,
sessions, and explicit Agent composition. It references Crystal and
Crystal.Agents.

### Crystal.Pipelines

Owns generic asynchronous operation and asynchronous event-stream middleware.
It has no project references and does not know model, Agent, or provider types.
Middleware is supplied by the caller and runs in declared order, wrapping the
terminal operation. The pipeline snapshots middleware at construction. It does
not create messages, intercept hidden global state, or persist results.
The same middleware delegates express caller policies: they may forward the
operation, reject it, or return an exact caller-owned result. Crystal defines
no separate policy framework or default exception-to-content mapping.
Optional diagnostics report only a caller-supplied stable operation name, a
start or terminal status, and elapsed time. Stream timing begins on enumeration
and ends on completion, cancellation, failure, or early disposal. Observer
callbacks are caller-owned and their exceptions propagate. Crystal provides no
telemetry exporter or backend.

### Crystal.Decorators

Owns typed adapters that apply generic middleware to provider-neutral Chat,
Completion, Embedding, multimodal Chat, and immediate image, audio, and video
generation clients. It references Crystal and Crystal.Pipelines, without a
dependency on executable Tools, Agents, or Harnesses. Wrapping preserves the
client's declared capability object and optional streaming interface. Supplying
stream middleware to a client without streaming support fails at construction.
The adapters forward exact requests, responses, events, and cancellation tokens
unless caller-supplied middleware explicitly changes them. The public `Clients`
entry point selects the wrapper for each client family.

Namespaces continue to express domain ownership. The Crystal.Tools namespace is
intentionally present in both Crystal and Crystal.Tools because its protocol
values belong at the adapter boundary while its executable infrastructure is an
optional higher layer.

## Namespace ownership

### Crystal

Owns small cross-capability values such as token usage and finish reasons.
Provider-originated values remain open so adapters do not lose information.

### Crystal.Reasoning

Owns request hints, readable reasoning text, readable-text classification, and
opaque continuation state.

ReasoningEffort is an open caller-authored value. Its named presets do not form
an exhaustive model-independent scale. Crystal preserves a custom value exactly
through requests, Agents, and Harnesses; external adapters map a supported value
or reject it. Crystal does not normalize one effort to a preset or infer a
provider parameter from its spelling.

One provider-native reasoning block maps to one ReasoningContent value. A block
contains zero or more readable text segments and optional opaque state. At least
one surface is required. Streaming deltas identify each readable segment with a
stable zero-based text-segment index so transport chunk boundaries do not erase
semantic segment boundaries.

Opaque state:

- has an adapter-defined format identifier;
- is copied on input and output;
- may encode encrypted content, signatures, redacted blocks, identifiers, or a
  complete provider-native envelope;
- is never parsed, combined, displayed, logged, or rewritten by Crystal; and
- is valid only for adapters that explicitly recognize its format.

### Crystal.Media

Owns explicit MIME types, codecs, dimensions, aspect ratios, and typed image,
audio, and video values. Media data is carried by one of three closed source
shapes:

- InlineMediaSource owns a private copy and returns copies to callers;
- UriMediaSource stores an absolute caller-supplied URI that Crystal never
  resolves or downloads; and
- ReplayableStreamMediaSource invokes a caller-owned factory for a fresh readable
  stream on every attempt, with returned-stream ownership transferring to the
  consumer.

Every source reports optional exact length and expiration metadata. Inline data
never expires; URI and replayable sources preserve a caller- or adapter-reported
ExpiresAt value without Crystal refreshing or fetching them.

Media values contain no file paths, provider resource identifiers, transport
DTOs, or automatic upload/download behavior. MIME is explicit. Optional media
metadata describes known facts and is not inferred by Crystal.

### Crystal.Multimodal

Owns the closed portable text, image, audio, and video content hierarchy, media
source-aware content capabilities, and multimodal reasoning content. It also
owns an independent non-streaming and optional typed streaming Chat family under
Crystal.Multimodal.Chat and model-facing multimodal tool calls and results under
Crystal.Multimodal.Tools.
Multimodal tool calls retain exact raw JSON arguments and optional ordered typed
content; results retain ordered caller-owned typed content.

Multimodal messages preserve ordered typed content blocks. Multimodal reasoning
preserves ordered readable typed parts, each with an open summary/trace
classification, plus opaque continuation state. The
multimodal Chat client advertises coarse individual input and output shapes.
Role-specific, cardinality, and conditional model rules remain adapter-owned.

### Crystal.Generation

Owns shared ordered typed conditioning inputs, portable input purposes, coarse
input/output capabilities, and ordered candidate items. Image, audio, and video
requests, hard requirements, responses, and immediate client interfaces live in
separate target-output namespaces so their lifecycles can evolve independently.

Generation input purposes are closed portable semantics: instruction, reference,
source, mask, first frame, and last frame. Image, audio, and video inputs remain
typed, including audio reference or source inputs supplied to video generation.
Editing or transformation is conditioned generation through source and mask
inputs; Crystal has no universal edit mode or edit client.

Requirements are hard constraints. An adapter must reject a requirement or input
combination it cannot honor and must not silently drop, approximate, reorder,
download, or transcode it. Provider-only controls belong on adapter APIs.

### Crystal.Embeddings

Owns ordered text batches, immutable vectors, responses, and IEmbeddingClient.
Response vector order corresponds to input order. The adapter is responsible for
ensuring response count and order match the request it processed.

### Crystal.Completions

Owns text prompts, ordered completion items, candidates, responses, typed stream
events, ICompletionClient, and IStreamingCompletionClient.

A completion candidate contains ordered text and reasoning items. Keeping
reasoning inside the ordered item sequence avoids losing provider output order.
Completion remains separate from Chat so adapters do not have to invent roles.

### Crystal.Chat

Owns ordered conversation items, text messages, roles, reasoning items, requests,
candidates, responses, typed stream events, IChatClient, and
IStreamingChatClient.

ChatItem is a protocol-item boundary. Current built-in natural-language content
is a ChatMessage containing one text string. Tool calls, tool results, and
reasoning are protocol items, not content modalities.

Multimodal Chat uses a separate explicit capability contract under
Crystal.Multimodal.Chat. IChatClient remains unchanged and text-only.

### Crystal.Tools

Owns:

- caller-authored ToolDefinition values;
- model-facing ToolCall and ToolResult protocol items;
- ITool and ToolOutput;
- immutable ToolCatalog lookup;
- IToolExecutor and the standard ToolExecutor;
- explicit serial or bounded-concurrent execution options; and
- optional caller-supplied invocation approval and exception mapping policies.

ToolDefinition, ToolCall, ToolResult, and ToolResultStatus are compiled into the
Crystal assembly. MultimodalToolCall, MultimodalToolResult, and
MultimodalToolResultStatus are also compiled into Crystal. The executable text
and multimodal infrastructure is compiled into Crystal.Tools.

The standard text and multimodal executors check every call name against the
immutable catalog before starting any call in a batch, whether dispatch is
serial or concurrent. They preserve input result order even when calls run
concurrently. Concurrent dispatch creates at most the configured number of
workers and stores each result at its original call index, so a large batch
does not create a waiting task for every call. Unknown tools, rejected calls
without caller-authored output, and unhandled tool exceptions terminate
execution. A later approval rejection or tool failure may occur after earlier
calls have started; the preflight
guarantee concerns registration only. Neither runtime writes an error message
or media block for the model.

After registration preflight, both executors check cancellation before each
call and after caller policy returns. These checks prevent a pending tool from
starting when cancellation has been observed, including when a caller policy
ignored its token. A post-tool check prevents returning success after observed
cancellation. Concurrent calls may already have passed their own check;
started side effects remain and are not rolled back. A canceled tool failure
bypasses optional exception-to-output mapping.

### Crystal.Agents

Owns IAgent, Agent, run inputs, optional limits, results, stop reasons, candidate
selection, and typed run events.

Agent executes this loop:

1. snapshot caller-supplied conversation items;
2. build a ChatRequest from the exact transcript and configured tool
   definitions;
3. invoke the injected IChatClient;
4. ask the caller-supplied selector to choose a candidate;
5. append every selected candidate item exactly and in order;
6. return when that candidate contains no tool call;
7. stop without adding text if a configured limit would be exceeded;
8. execute all selected tool calls through the configured IToolExecutor;
9. append correlated results in call order; and
10. continue until normal completion, cancellation, failure, or a limit stop.

Model and tool calls are counted when attempted. Tool batches are all-or-none
with respect to the configured tool-call budget: Agent never starts a partial
batch merely because some budget remains.
Each Agent limit is independent. A null maximum means no configured bound for
that dimension, and Unlimited sets all three maximums to null. A finite
duration creates a cancellation timer; an unlimited duration relies on caller
cancellation. Attempts remain counted even when their limit is unlimited.

Agent returns aggregated usage only when every attempted model call reports
usage. If a model call times out or any completed response omits usage, the run
usage is null. When every response reports usage but any response omits a
reasoning-token count, only the aggregated reasoning-token count is null.

Agent consumes IStreamingChatClient when the configured client supports it,
forwards each exact ChatStreamEvent, and assembles a complete ChatResponse
before candidate selection and tool execution. Other clients use non-streaming
IChatClient responses. Invalid or incomplete streams fail without appending
partial model content to the transcript. Both paths preserve the same Agent
limits, tool ordering, and usage rules.

The independent Crystal.Multimodal.Agents family applies the same explicit loop
to IMultimodalChatClient and IMultimodalToolExecutor. Its request, limits,
selector, events, result, stop reasons, and interface do not widen or inherit the
text Agent contracts. When the configured client also implements
IStreamingMultimodalChatClient, the Agent forwards every exact client stream
event, assembles a semantically equivalent complete response, and then applies
the same candidate-selection and tool-execution path. Otherwise it uses the
non-streaming operation. It replays selected media values exactly. The runtime
does not open URI sources, inspect media bytes, transcode, upload, download, or
cache media. IMultimodalAgent exposes the snapshotted input and output
capabilities of its configured model client. Callers must keep URI and
replayable-stream sources valid for the entire run.

### Crystal.Harness

Owns AgentName, registration, Harness limits, sessions, explicit invocations,
ancestry, results, and forwarded events.

AgentHarness is an immutable registry. Creating a session establishes one shared
budget and cancellation boundary. The caller explicitly invokes registered
Agents and supplies parent invocation identifiers. The session:

- validates ancestry and maximum depth;
- reserves model-call and tool-call capacity before concurrent invocations;
- gives each Agent an effective limit no larger than its request or the shared
  remainder;
- returns unused reserved capacity after a successful run;
- uses one shared wall-clock duration boundary when configured;
- propagates session and invocation cancellation; and
- wraps Agent events with session, Agent, invocation, and parent identifiers.

Each Harness maximum may be null, meaning no configured shared bound. Unlimited
sets depth, model calls, tool calls, and duration to null. A finite session
maximum narrows an unlimited per-Agent request; an unlimited shared maximum
retains a finite per-Agent request. Only finite shared call capacity is reserved
and returned. Caller and session cancellation continue to propagate when
duration is unlimited.

No model output automatically routes to another Agent. Callers build routers,
graphs, supervisors, handoffs, or peer topologies around the explicit invocation
boundary.

Harness sessions are in-memory execution boundaries. The caller owns durable
conversation history, session storage, and restoration across process lifetimes.
Crystal does not provide a checkpoint store or restore a prior Harness session.

Crystal.Multimodal.Harness is an independent registry, session, reservation,
invocation, event, and result family for IMultimodalAgent. Text and multimodal
Agents cannot be mixed accidentally in one built-in registry. Both families
apply the same explicit shared-budget and ancestry semantics.

## Public contract principles

- Public data values are immutable.
- Mutable input collections are snapshotted.
- Ordered data is never sorted, grouped, or deduplicated implicitly.
- Public correlation values reject contradictory identifiers, names, and ancestry.
- Provider-originated open values retain their raw string.
- Provider options remain in adapter APIs rather than extension dictionaries.
- Configured clients own provider and model selection.
- Expected model termination and configured limit stops are data.
- Invalid contracts, adapter failures, unhandled tool failures, and broken
  implementations are exceptions.
- Cancellation is propagated and is never converted into an ordinary failure.
- A pipeline may change a request, response, or event only through explicitly
  supplied caller middleware; the pipeline runtime itself preserves them.

## Streaming semantics

Text and multimodal provider streaming use typed IAsyncEnumerable<T> events.
Candidate and item indexes preserve interleaving. Text reasoning deltas
additionally carry a text-segment index; every delta for one semantic segment
uses the same index. Multimodal message content, reasoning parts, and tool-call
content carry stable zero-based indexes. A message-started event preserves its
role and permits an empty content sequence. Text blocks may arrive as exact
deltas or as one complete content event. Image, audio, and video arrive as one
complete content event; this contract does not stream media bytes.

Identifier, name, argument, text, and reasoning deltas are explicitly identified
as deltas; adapters must not pretend partial data is complete. A content-received
event is explicitly complete and cannot be combined with text deltas at the same
content or reasoning-part index.

A complete stream must be aggregatable into the same semantic response as the
non-streaming operation. Opaque state may be buffered by an adapter and emitted
as a completed state event.

Text and multimodal Agent and Harness streams end with a typed completion event
containing the same result returned by their non-streaming methods. Consumer
cancellation stops enumeration and propagates to in-flight model, policy, and
tool operations.

Multimodal Chat streaming describes the delivery of typed Chat content; it does
not imply a generic media-byte stream. Generated-media previews, byte chunks,
resumable remote operations, and realtime sessions require separate future
contracts with portable lifecycle semantics.

## Safety and disclosure

- No mutable global registries or ambient service locators.
- No automatic retry, prompt repair, candidate heuristic, context reduction, or
  tool exception disclosure.
- No partial tool batch execution caused by a remaining-budget calculation.
- Concurrent tool execution requires an explicit bounded concurrency setting.
- Reasoning text, opaque state, raw tool arguments, prompts, media bytes, media
  URIs, and tool exception details are absent from Crystal-authored diagnostics.
- Policies that create model-visible text are supplied by the caller and their
  returned text is replayed exactly.

## Media and generation boundary

The current media architecture is additive:

- existing text contracts remain valid and text-only;
- multimodal Chat, Tool, Agent, and Harness APIs are independent families;
- typed image, audio, and video values replace an untyped attachment bag;
- inline-copy, absolute-URI, and replayable-stream ownership is explicit;
- target-output generation clients remain separate;
- closed typed inputs can condition every generation target when an adapter
  advertises that shape;
- a generated video reports embedded-audio presence, while a separately generated
  audio value remains a distinct ordered output item; and
- capability profiles are intentionally coarse and never claim to encode every
  provider model constraint.

Immediate single-request generation, batch submission, generated-media
streaming, resumable remote operations, and realtime sessions are distinct
lifecycles. Only immediate single-request generation is in
the current production contract. Local cancellation cancels waiting and
in-flight cooperative work; it must not be documented as remote-job cancellation
when an adapter has already submitted a persistent provider operation.

No production type is a placeholder media abstraction, generic option bag,
provider resource handle, or universal edit mode.

## Dependency and serialization boundary

The Crystal project retains its existing Newtonsoft.Json, Newtonsoft.Json.Bson,
and System.Text.Json references by explicit user decision. The higher-layer
projects add only project references. Domain contracts carry no provider
serialization attributes. Tool schemas use System.Text.Json JsonElement, and
model-generated tool arguments remain raw text until a tool chooses to parse
them.
