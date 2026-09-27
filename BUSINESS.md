# Crystal Product Definition

## Mission

Crystal is a provider-neutral, prompt-neutral, and tool-neutral C# library
family for integrating text and multimodal models, media generation, and
building inspectable Agents and Agent Harnesses. It defines stable protocol
contracts and deterministic execution infrastructure while leaving models,
transport, prompts, tools, policies, and application state to external code.

Crystal consists of reusable libraries. It does not host an application, expose
a service, select a model, or own an end-user experience. Its growth is driven by
portable use cases and explicit semantics, not compatibility with another
library's public API.

## Intended users

- Provider-adapter authors implementing portable text, multimodal, or generation
  contracts.
- Application developers using completion, chat, embedding, or media generation
  capabilities.
- Tool authors exposing caller-owned capabilities to an Agent.
- Agent authors composing model calls and tools under explicit policies.
- Harness authors coordinating parent and child Agents without a built-in
  routing topology.
- Workflow authors defining typed execution graphs that can include caller
  functions or Agent invocations as nodes.

## Production assemblies

- Crystal contains provider-adapter contracts, the text and multimodal protocol
  foundations, media values, multimodal Embedding, and immediate, streaming,
  resumable operation, and batch generation clients, and live media session
  contracts.
- Crystal.Tools adds independent text and multimodal tool registration, policy,
  and execution families.
- Crystal.Agents adds independent text and multimodal model/tool loops with
  caller-configured optional limits.
- Crystal.Harness adds independent text and multimodal Agent composition and
  shared limits.
- Crystal.Pipelines adds type-safe, caller-owned asynchronous operation and
  streaming middleware composition without depending on a particular model,
  tool, or provider contract.
- Crystal.Decorators wraps current provider-neutral client families with
  caller-owned middleware while preserving their supported interfaces and
  capability profiles.
- Crystal.Workflows executes caller-defined typed graph nodes and conditional
  edges without depending on model, Agent, provider, or persistence contracts.

Consumers reference only the layers required by their use case. A provider
adapter can implement text, multimodal, or generation capabilities without
depending on tool execution, Agent runtime, or Harness composition.

## Current capabilities

### Embedding

- Ordered batches of text inputs.
- Ordered immutable floating-point vectors.
- Optional provider-reported usage.
- One asynchronous provider-neutral client contract.

### Multimodal Embedding

- An independent optional client; text Embedding remains text-only.
- Each input contains non-empty ordered typed text, image, audio, or video
  content, and a request contains a non-empty ordered batch of inputs.
- Coarse input capabilities advertise individual modalities and media source
  shapes. Adapters reject unsupported requests and return exactly one vector
  per request input in the same order.
- Existing immutable embedding vectors and optional provider-reported token
  usage remain usable; Crystal does not inspect or transform media.

### Completion

- Caller-authored text prompts.
- Ordered text and reasoning output items per candidate.
- Multiple candidates and open-ended finish reasons.
- Non-streaming and optional typed streaming client contracts.

### Chat

- Ordered text messages with open-ended roles.
- Ordered reasoning, tool-call, and tool-result protocol items.
- Multiple candidates and open-ended finish reasons.
- Non-streaming and optional typed streaming client contracts.

### Structured final text

- Caller-authored JSON Schema is an optional hard output requirement for
  Completion and text or multimodal Chat.
- Agent and Harness requests carry the same requirement unchanged to each model
  turn. Crystal does not add formatting prompts or repair invalid model output.
- The requirement governs a normal final text candidate, independently of tool
  argument schemas. An adapter rejects an unsupported request or reports a
  provider failure if the response violates the requirement.

### Media and multimodal Chat

- Closed portable text, image, audio, and video content modalities.
- Explicit MIME types and typed image, audio, and video metadata.
- Immutable inline bytes, caller-owned absolute URIs, and replayable stream
  factories with explicit ownership and optional expiration metadata.
- Coarse input and output capabilities that include accepted media source shapes.
- Independent non-streaming and optional typed streaming multimodal Chat client
  contracts.
- Ordered multimodal messages, reasoning, tool calls, and tool results.
- Stable candidate, item, content-block, and reasoning-part identity in
  multimodal Chat streams.

### Immediate media generation

- Independent image, audio, and video generation client contracts.
- Shared ordered typed text, image, audio, and video inputs with portable
  instruction, reference, source, mask, first-frame, and last-frame purposes.
- Editing and transformation represented by conditioned source inputs rather
  than a universal edit mode.
- Portable hard output requirements; unsupported requirements must be rejected.
- Ordered interleaved text, image, audio, video, and reasoning output.
- Audio references for video generation and explicit embedded-audio presence.

### Resumable remote generation operations

- Separate optional image, audio, and video operation client interfaces accept
  the corresponding generation requests without changing immediate clients.
- A start or poll returns one state snapshot with an opaque adapter-owned ticket.
  The ticket may change; callers retain the latest ticket and resupply it to a
  compatible adapter instance. Crystal does not save or inspect it.
- Pending, running, completed, failed, and canceled are portable remote states.
  Only completed carries the exact target-specific generation response.
- Poll cadence, durable storage, retries, and any remote cancellation are
  caller and adapter concerns. Canceling local waiting does not cancel an
  accepted remote operation.

### Generation batch submission

- Separate optional image, audio, and video batch clients submit one non-empty
  ordered collection of exact target-specific requests as a remote operation.
- A completed batch contains exactly one terminal item result per input at the
  same index. An item may have a complete response, fail, or be canceled;
  partial item failure does not erase other item results.
- Batch start and poll return the same opaque-ticket operation snapshots.
  Crystal does not split batches, persist tickets, choose retries, or emulate a
  batch with repeated immediate calls.

### Generated-media streaming

- Separate optional image, audio, and video stream clients receive the same
  target-specific generation requests without widening immediate clients.
- Streams may interleave complete provisional media previews and encoded media
  chunks across candidate and item indexes. Revisions are explicit, so later
  provisional output never silently rewrites an earlier revision.
- Chunks carry copied bytes, explicit MIME and modality, contiguous indexes
  within a revision, and a final-chunk marker. A completed revision is a
  provisional encoded media value, not necessarily the final artifact.
- A successful stream ends with exactly one event containing the authoritative
  complete target-specific generation response. Error and cancellation end by
  exception or canceled enumeration without inventing a response.

### Realtime media sessions

- An independent duplex client opens one live session with caller-selected
  output modalities, exact optional initial context and tools, and an explicit
  automatic or caller-ended turn mode. Unsupported combinations are rejected.
- The caller serializes exact text or independently decodable media segments
  into SendAsync while one ReceiveAsync enumeration may run concurrently.
  Ordered output segments carry stable output identifiers and segment indexes.
- Separate events preserve exact model tool calls, caller tool results, and
  reasoning blocks. Crystal does not execute tools, synthesize prompts, infer
  turn boundaries, assemble a transcript, or reconnect a session.
- CloseAsync releases the live local session. Persistent conversation state and
  any connection-resumption mechanism remain caller and adapter concerns.

### Reasoning

- Provider-neutral request hints for mode, effort, visible output, and budget.
  Effort presets are optional conveniences; callers can supply another effort
  value, which external adapters must interpret or reject explicitly.
- Readable reasoning text classified as summary or trace.
- Opaque continuation state copied and replayed unchanged.
- Ordered preservation across ordinary and tool-calling turns.
- Stable candidate, item, and text-segment identity in reasoning streams.

### Tools

- Caller-authored definitions and JSON input schemas.
- Raw model-generated argument text.
- Immutable catalogs and explicit serial or concurrent dispatch.
- Whole-batch registration checks before any text or multimodal tool starts.
- Optional caller-owned approval and exception-to-output policies.
- Textual outputs correlated to model tool calls.
- A separate multimodal tool family with optional ordered typed call content,
  ordered typed outputs, and the same
  explicit approval, exception disclosure, and scheduling choices.

### Agent

- A prompt-free model/tool loop.
- Independently finite or unlimited model-call, tool-call, and duration limits.
- Caller-supplied candidate selection.
- Typed events containing exact model requests, responses, and tool results.
- Exact transcript preservation and explicit stop reasons.
- Aggregated usage only when every attempted model call reports usage.
- Text Agents consume typed Chat streams when the configured client supports
  them, forward exact stream events, and assemble a complete response before
  candidate selection and tool execution.
- A separate multimodal Agent family that replays media values exactly and does
  not fetch, transcode, or cache them.
- Multimodal Agents consume provider streams when available, forward exact
  stream events, and assemble the same complete response used by candidate
  selection and tool execution.

### Harness

- Named Agent registration.
- Explicit parent-child invocation.
- Shared depth, model-call, tool-call, duration, and cancellation boundaries.
- Each Harness budget dimension can be finite or explicitly unlimited.
- Invocation ancestry and event forwarding.
- No built-in router, supervisor prompt, or persistence store; graph execution
  belongs to the independent Workflows assembly.
- A separate multimodal Harness registry, session, budget, event, and result
  family.

### Workflows

- Type-matched nodes and edges for caller-owned asynchronous operations.
- Conditional per-message routing and fan-out without model-authored policy.
- Bounded concurrent execution of nodes in a superstep, with deterministic
  node and edge ordering when collecting outputs and routing messages.
- Messages reaching one node in the same superstep are supplied as one ordered
  batch. A longer branch can reach that node in a later superstep and invoke it
  again; cross-step barriers require caller-owned node logic.
- Optional finite or unlimited superstep count, ordered metadata-only routing
  events, terminal outputs, and explicit limit-stop results.
- No built-in prompts, graph topology, checkpointing, session storage, or
  human-interaction protocol; the caller supplies the topology and operations.

### Operation pipelines

- Ordered middleware composition for any asynchronous request and response.
- Independent ordered middleware composition for asynchronous event streams.
- Exact request, response, event, exception, and cancellation behavior unless
  caller-owned middleware explicitly changes it.
- Caller-owned middleware can forward, reject, or return an exact caller-owned
  result. Crystal supplies no built-in policy decision or model-visible text.
- Opt-in timing and outcome observations without request, response, event, or
  exception payloads; the observer and its sink remain caller-owned.
- Opt-in exception retry middleware with an explicit total-attempt bound and a
  caller-owned decision after each failure. The same request object is replayed;
  callers own cost, replay safety, and any repeated side effects.
- Typed wrappers for Chat, Completion, text and multimodal Embedding,
  multimodal Chat, and immediate image, audio, and video generation. A wrapper
  exposes streaming only when its underlying client does.
- Opt-in text and multimodal Embedding cardinality checks reject an adapter
  response that lacks exactly one vector for each input.
- Opt-in multimodal Embedding input preflight checks only declared individual
  modalities and media source shapes. Model-specific combinations remain the
  external adapter's responsibility.

## Meaning of neutral

### Prompt-neutral

Every model-bound natural-language string must originate from:

- caller input;
- exact prior model output; or
- exact output from a caller-registered tool or policy.

Crystal may add roles, call identifiers, event sequence numbers, run identifiers,
finish reasons, and other protocol metadata. It may not author language for the
model.

### Provider-neutral

Provider configuration and wire behavior live outside Crystal. A configured
adapter chooses its service and model. Crystal contracts contain no endpoint,
credential, vendor option, vendor DTO, or raw SDK response.

Common semantic hints are portable requests, not promises. An adapter documents
its mapping and rejects explicitly unsupported semantics.

### Tool-neutral

Crystal can describe, locate, approve, invoke, correlate, and schedule tools. It
ships no tool that performs a useful application or external action. Registering
a tool is an explicit caller decision.

## Current exclusions

The current release does not include:

- generic binary attachments, PDFs, or general file-content bags;
- automatic URI fetching, media upload, download, transcoding, or caching;
- built-in providers, authentication, transport, or model catalogs;
- built-in prompts, personas, templates, repair messages, or summaries;
- built-in search, filesystem, shell, clock, network, or other concrete tools;
- retrieval stores, memory stores, persistence, hosting, UI, or telemetry
  backends;
- framework-owned session storage, checkpointing, or restoration; applications
  retain and resupply the state needed for later invocations;
- automatic retries, context truncation, routing, planning, or side-effect
  approval.

## Current modality boundary

Multimodal and media-generation support is additive:

- text-only interfaces remain text-only;
- multimodal Chat, Tool, Agent, and Harness families are independent;
- image, audio, and video are typed first-class values rather than attachments;
- generation clients are separated by target output while accepting a shared
  closed set of typed conditioning inputs;
- capability profiles state portable individual input and output shapes, while
  adapters validate model-specific combinations and cardinality;
- provider options remain on adapter APIs rather than in extension dictionaries;
  and
- immediate single-request, batch, streaming, resumable-operation, and realtime
  lifecycles are not collapsed into one universal generation interface.

## Acceptance criteria for the text foundation

The text foundation is usable when a developer can:

1. implement completion, chat, and embedding adapters without Agent internals;
2. preserve every supported reasoning block and opaque continuation payload;
3. register and execute caller-defined tools without Crystal-authored text;
4. run a bounded tool-calling Agent and inspect every protocol transition;
5. distinguish normal completion from every configured limit stop;
6. compose named parent and child Agents under shared Harness budgets;
7. cancel model, tool, Agent, and Harness work cooperatively; and
8. consume only the production assemblies it needs without receiving a provider,
   prompt, or concrete tool.
