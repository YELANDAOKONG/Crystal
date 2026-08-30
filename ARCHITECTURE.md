# Crystal Architecture

## Status

This document is the authoritative architecture for the current text and
multimodal development line. There is no compatibility baseline yet, so public
names may change while the design documents and implementation change together.

## Dependency direction

Crystal ships as four production assemblies with one-way project references:

~~~text
Crystal.Harness
    ↓
Crystal.Agents
    ↓
Crystal.Tools
    ↓
Crystal
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
owns an independent non-streaming Chat family under Crystal.Multimodal.Chat and
model-facing multimodal tool calls and results under Crystal.Multimodal.Tools.
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

The standard text and multimodal executors preserve input call order even when
calls run concurrently. Unknown tools, rejected calls without caller-authored
output, and unhandled tool exceptions terminate execution. Neither runtime
writes an error message or media block for the model.

### Crystal.Agents

Owns IAgent, Agent, run inputs, finite limits, results, stop reasons, candidate
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

Agent returns aggregated usage only when every attempted model call reports
usage. If a model call times out or any completed response omits usage, the run
usage is null. When every response reports usage but any response omits a
reasoning-token count, only the aggregated reasoning-token count is null.

Agent uses non-streaming IChatClient responses for model turns. Its own
IAsyncEnumerable event stream observes turn boundaries and exact protocol
objects. Direct provider streaming remains available through
IStreamingChatClient.

The independent Crystal.Multimodal.Agents family applies the same explicit loop
to IMultimodalChatClient and IMultimodalToolExecutor. Its request, limits,
selector, events, result, stop reasons, and interface do not widen or inherit the
text Agent contracts. It replays selected media values exactly. The runtime does
not open URI sources, inspect media bytes, transcode, upload, download, or cache
media. IMultimodalAgent exposes the snapshotted input and output capabilities
of its configured model client. Callers must keep URI and replayable-stream
sources valid for the entire run.

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
- uses one shared wall-clock duration boundary;
- propagates session and invocation cancellation; and
- wraps Agent events with session, Agent, invocation, and parent identifiers.

No model output automatically routes to another Agent. Callers build routers,
graphs, supervisors, handoffs, or peer topologies around the explicit invocation
boundary.

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

## Streaming semantics

Current text provider streaming uses typed IAsyncEnumerable<T> events. Candidate
and item indexes preserve interleaving. Reasoning text deltas additionally carry a
text-segment index; every delta for one semantic segment uses the same index.
Identifier, name, argument, text, and reasoning deltas are explicitly identified
as deltas; adapters must not pretend partial data is complete.

A complete stream must be aggregatable into the same semantic response as the
non-streaming operation. Opaque state may be buffered by an adapter and emitted
as a completed state event.

Text and multimodal Agent and Harness streams end with a typed completion event
containing the same result returned by their non-streaming methods. Consumer
cancellation stops enumeration and propagates to in-flight model, policy, and
tool operations.

No generic media streaming contract is implied by immediate generation or
non-streaming multimodal Chat. Generated-media previews, byte chunks, resumable
remote operations, and realtime sessions require separate future contracts with
portable lifecycle semantics.

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
