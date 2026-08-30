# Crystal Product Definition

## Mission

Crystal is a provider-neutral, prompt-neutral, and tool-neutral C# library
family for integrating text and multimodal models, media generation, and
building inspectable Agents and Agent Harnesses. It defines stable protocol
contracts and deterministic execution infrastructure while leaving models,
transport, prompts, tools, policies, and application state to external code.

Crystal consists of reusable libraries. It does not host an application, expose
a service, select a model, or own an end-user experience.

## Intended users

- Provider-adapter authors implementing portable text, multimodal, or generation
  contracts.
- Application developers using completion, chat, embedding, or media generation
  capabilities.
- Tool authors exposing caller-owned capabilities to an Agent.
- Agent authors composing model calls and tools under explicit policies.
- Harness authors coordinating parent and child Agents without a built-in
  routing topology.

## Production assemblies

- Crystal contains provider-adapter contracts, the text and multimodal protocol
  foundations, media values, and immediate generation clients.
- Crystal.Tools adds independent text and multimodal tool registration, policy,
  and execution families.
- Crystal.Agents adds independent bounded text and multimodal model/tool loops.
- Crystal.Harness adds independent text and multimodal Agent composition and
  shared limits.

Consumers reference only the layers required by their use case. A provider
adapter can implement text, multimodal, or generation capabilities without
depending on tool execution, Agent runtime, or Harness composition.

## Current capabilities

### Embedding

- Ordered batches of text inputs.
- Ordered immutable floating-point vectors.
- Optional provider-reported usage.
- One asynchronous provider-neutral client contract.

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

### Media and multimodal Chat

- Closed portable text, image, audio, and video content modalities.
- Explicit MIME types and typed image, audio, and video metadata.
- Immutable inline bytes, caller-owned absolute URIs, and replayable stream
  factories with explicit ownership and optional expiration metadata.
- Coarse input and output capabilities that include accepted media source shapes.
- An independent non-streaming multimodal Chat protocol and client contract.
- Ordered multimodal messages, reasoning, tool calls, and tool results.

### Immediate media generation

- Independent image, audio, and video generation client contracts.
- Shared ordered typed text, image, audio, and video inputs with portable
  instruction, reference, source, mask, first-frame, and last-frame purposes.
- Editing and transformation represented by conditioned source inputs rather
  than a universal edit mode.
- Portable hard output requirements; unsupported requirements must be rejected.
- Ordered interleaved text, image, audio, video, and reasoning output.
- Audio references for video generation and explicit embedded-audio presence.

### Reasoning

- Provider-neutral request hints for mode, effort, visible output, and budget.
- Readable reasoning text classified as summary or trace.
- Opaque continuation state copied and replayed unchanged.
- Ordered preservation across ordinary and tool-calling turns.
- Stable candidate, item, and text-segment identity in reasoning streams.

### Tools

- Caller-authored definitions and JSON input schemas.
- Raw model-generated argument text.
- Immutable catalogs and explicit serial or concurrent dispatch.
- Optional caller-owned approval and exception-to-output policies.
- Textual outputs correlated to model tool calls.
- A separate multimodal tool family with optional ordered typed call content,
  ordered typed outputs, and the same
  explicit approval, exception disclosure, and scheduling choices.

### Agent

- A prompt-free model/tool loop.
- Required finite model-call, tool-call, and duration limits.
- Caller-supplied candidate selection.
- Typed events containing exact model requests, responses, and tool results.
- Exact transcript preservation and explicit stop reasons.
- Aggregated usage only when every attempted model call reports usage.
- A separate multimodal Agent family that replays media values exactly and does
  not fetch, transcode, or cache them.

### Harness

- Named Agent registration.
- Explicit parent-child invocation.
- Shared depth, model-call, tool-call, duration, and cancellation boundaries.
- Invocation ancestry and event forwarding.
- No built-in router, supervisor prompt, graph, or persistence store.
- A separate multimodal Harness registry, session, budget, event, and result
  family.

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
- batch submission, generated-media streaming, resumable remote generation
  operations, or realtime media sessions;
- automatic URI fetching, media upload, download, transcoding, or caching;
- built-in providers, authentication, transport, or model catalogs;
- built-in prompts, personas, templates, repair messages, or summaries;
- built-in search, filesystem, shell, clock, network, or other concrete tools;
- retrieval stores, memory stores, persistence, hosting, UI, or telemetry
  backends;
- automatic retries, context truncation, routing, planning, or side-effect
  approval.

## Current modality boundary

Multimodal and immediate media generation support is additive:

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
