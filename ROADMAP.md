# Crystal Roadmap

## Current direction

Crystal is a new net10.0 library with no compatibility baseline. The current
development line now includes the additive multimodal and immediate media
generation foundation while preserving every text-only interface.

Large public API changes are allowed while this roadmap reports no preview
baseline. Design documents and code must change together.

## Recorded decisions

- 2026-08-22: Crystal has no built-in prompts, providers, or concrete tools.
- 2026-08-22: Reasoning is ordered protocol data with readable and opaque
  surfaces.
- 2026-08-23: Breaking changes are allowed because no consumer depends on the
  project.
- 2026-08-23: The initial implementation scope was text-only.
- 2026-08-23: Image, audio, and video capabilities remain committed future
  directions and must be added through explicit additive interfaces.
- 2026-08-23: Existing JSON package references remain unchanged.
- 2026-08-23: No unit-test project is currently authorized.
- 2026-08-23: The existing net10.0 target remains unchanged.
- 2026-08-23: Reasoning stream deltas identify semantic text segments explicitly.
- 2026-08-23: Agent usage is present only when every model attempt reports it.
- 2026-08-27: Production code is split into Crystal, Crystal.Tools,
  Crystal.Agents, and Crystal.Harness with one-way project references.
- 2026-08-27: Model-facing tool protocol values remain in Crystal so provider
  adapters do not depend on executable tool infrastructure.
- 2026-08-30: Phase 6 is additive. Existing text interfaces remain unchanged and
  text-only.
- 2026-08-30: Multimodal Chat, Tool, Agent, and Harness families are independent
  from their text counterparts.
- 2026-08-30: Image, audio, and video generation use independent target-output
  clients over shared ordered typed inputs. Editing is conditioned generation,
  not a separate universal lifecycle or mode.
- 2026-08-30: Video generation can accept audio reference or source inputs when
  the adapter advertises that capability.
- 2026-08-30: Portable capability profiles remain coarse. External adapters own
  conditional model rules and reject unsupported hard requirements.
- 2026-08-30: Immediate generation, generated-media streaming, resumable remote
  operations, and realtime sessions remain distinct lifecycles.
- 2026-09-22: Multimodal Chat gains an optional typed streaming contract.
  Message text is incremental; other typed content is delivered complete.
  Multimodal Agent forwards provider events and assembles the complete response
  before candidate selection and tool execution.
- 2026-09-27: Growth targets portable capabilities rather than API compatibility
  or migration support for named frameworks.
- 2026-09-27: Session storage, checkpointing, and process-spanning restoration
  remain application responsibilities.
- 2026-09-27: The user authorized an xUnit test project and additional class
  library projects. Crystal.Pipelines begins generic middleware composition.
- 2026-09-27: Crystal.Decorators is the opt-in client wrapper assembly. Caller
  policies use ordinary generic middleware delegates; no parallel policy
  abstraction is required.
- 2026-09-27: Agent and Harness budgets can be independently finite or
  unlimited. Null maximums mean no configured bound; Unlimited presets expose
  the all-unlimited case for text and multimodal families.
- 2026-09-27: Reasoning-effort presets are conveniences, not a closed set.
  Caller-defined effort values pass through unchanged; external adapters own
  support checks and rejection.
- 2026-09-27: Text and multimodal tool dispatch checks cancellation at policy
  and invocation boundaries, including when caller implementations ignore the
  token. Pending calls that observe cancellation do not start, and canceled
  failures skip exception mapping.
- 2026-09-27: Concurrent text and multimodal tool dispatch uses at most the
  configured number of workers, with results stored in original call order.

## Phase 0 — Product and architecture reset

Status: complete.

Deliverables:

- one vocabulary for Completion, Chat, Tool, Agent, and Harness;
- explicit current exclusions and future modality direction;
- provider-adapter reasoning requirements; and
- executable design rules for prompt neutrality and explicit execution limits.

## Phase 1 — Text-model protocol

Status: complete.

Deliverables:

- immutable common values;
- ordered text Embedding inputs and vectors;
- ordered Completion text and reasoning items;
- ordered Chat messages, reasoning, tool calls, and tool results;
- non-streaming and optional typed streaming client interfaces;
- lossless reasoning text-segment identity in streams; and
- lossless opaque reasoning continuation.

Exit criteria: external adapters can implement all supported model capabilities
without depending on tool execution or Agent runtime internals.

## Phase 2 — Tool infrastructure

Status: complete.

Deliverables:

- executable caller-owned tools;
- immutable case-sensitive catalog;
- registration preflight for complete text and multimodal batches;
- cooperative cancellation checks at tool-dispatch boundaries;
- explicit serial and bounded-worker concurrent execution;
- ordered correlation of results;
- optional caller-owned approval; and
- optional caller-owned exception-to-output mapping.

Exit criteria: an application can execute a batch of registered tools without
Crystal producing model-visible text.

## Phase 3 — Agent runtime

Status: complete.

Deliverables:

- immutable run request, limits, result, and stop-reason contracts;
- prompt-free model/tool loop;
- caller-supplied candidate selection;
- exact transcript replay;
- typed model and tool transition events; and
- cooperative cancellation and duration limits.

Exit criteria: a recording adapter can prove that a multi-turn tool run contains
only caller, selected-model, and registered-tool text.

## Phase 4 — Harness composition

Status: complete.

Deliverables:

- named Agent registry;
- Harness sessions with independent optional shared limits;
- explicit parent-child invocation;
- shared budget reservation and cancellation;
- ancestry and event forwarding; and
- no built-in routing topology.

Exit criteria: callers can compose sequential or concurrent Agent trees while
the Harness enforces shared limits.

## Architecture refinement — Assembly decomposition

Status: complete.

Deliverables:

- a provider-adapter protocol assembly with no project references;
- optional Tool, Agent, and Harness runtime assemblies;
- one-way, acyclic project references;
- unchanged public namespaces and runtime semantics; and
- shared build configuration without new package dependencies.

## Phase 5 — Quality baseline

Status: in progress; xUnit test project authorized.

Deliverables:

- add unit, protocol, Agent, and Harness contract tests;
- add API compatibility tooling;
- decide package metadata and CI;
- review whether all current JSON dependencies remain necessary; and
- establish the first public compatibility baseline.

Current progress: the xUnit suite covers generic pipelines, client wrappers,
text and multimodal Agent streaming, media-source ownership, text Harness
ancestry and shared-budget behavior, and text/multimodal tool registration
preflight and cancellation. Concurrent tool scheduling tests cover worker
bounds and result order in both tool families. The compatibility baseline,
package metadata, CI, and broader contract coverage remain open.

Until the suite covers existing contracts, the solution build remains the
repository-wide verification gate.

## Phase 6 — Multimodal and media generation

Status: multimodal Chat, optional Chat streaming, and immediate-generation scope
complete.

Completed deliverables:

1. explicit inline-copy, absolute-URI, and replayable-stream media semantics,
   including optional source expiration;
2. typed image, audio, and video values with explicit MIME and known metadata;
3. closed typed multimodal content and coarse input/output capability profiles;
4. independent non-streaming and optional typed streaming multimodal Chat plus
   multimodal Tool protocol contracts;
5. independent executable multimodal Tool, Agent, and Harness families;
6. independent immediate image, audio, and video generation clients;
7. ordered text, image, audio, video, and reasoning generation output;
8. typed source, reference, mask, first-frame, last-frame, and audio-for-video
   inputs;
9. portable hard output requirements with adapter-owned rejection semantics;
10. lossless streaming indexes for messages, reasoning parts, and tool-call
    content, with complete typed media events; and
11. multimodal Agent stream forwarding and complete-response assembly.

Deferred Phase 6 lifecycles:

1. explicit batch-generation submission and result semantics;
2. modality-specific generated-media streaming and preview semantics;
3. resumable long-running operation handles, polling, persistence, and explicit
   remote cancellation semantics;
4. stateful realtime audio and video sessions; and
5. broader automated protocol and runtime tests under the now-authorized xUnit
   project; media-source ownership and multimodal Agent streaming/tool replay
   tests are implemented.

No Phase 6 production type may be a placeholder media abstraction, generic
option bag, provider resource handle, or universal edit mode.

## Phase 7 — General operation composition

Status: complete.

Deliverables:

1. generic typed asynchronous middleware composition;
2. independent middleware composition for typed asynchronous streams;
3. tests for ordering, cancellation, failures, and construction invariants;
4. explicit adapters from existing client families where they add value without
   changing the model-facing protocol; and
5. opt-in content-free diagnostics and caller-configured policies through the
   same generic middleware delegates, with no implicit exception disclosure.

Operation and streaming middleware, content-free timing and outcome diagnostics,
typed adapters for all current client families, and behavioral tests are
implemented. Caller policies can forward, reject, or return exact caller-owned
output without a provider dependency or Crystal-authored model text.

Exit criteria: callers can compose cross-cutting behavior around supported
operations without a provider dependency or Crystal-authored model text.

## Phase 8 — Portable model and tool semantics

Status: in progress; text and multimodal Agent stream consumption is implemented.

Evaluate structured output, richer embedding inputs, and tool schema binding
against multiple distinct provider shapes. Text Agent stream consumption
forwards exact events and assembles the complete response before selection.
Add only semantics that can be specified without a provider identifier,
wire DTO, or generic option bag. Each addition needs lossless ordering rules,
unsupported-feature behavior, and stream/non-stream equivalence tests.

Current progress: text and multimodal Agent tests compare streamed and complete
responses for equivalent run outcomes, usage, and ordered output; multimodal
coverage also checks exact media-value preservation. Open caller-defined
reasoning effort is documented and tested across protocol and runtime layers.
Broader protocol semantics remain under evaluation before adding public
contracts.

Exit criteria: an external adapter can implement each added contract without
referencing Agent internals, and an unsupported request fails explicitly.

## Phase 9 — Opt-in runtime policies

Status: planned.

Build caller-configured context selection, retries, caching, routing,
observability, and human-input boundaries on explicit operation interfaces.
Every policy declares its effect on requests, responses, cost, and side effects.
No built-in policy authors model-bound language. Applications own any durable
conversation history and supply the state for each new invocation.

Exit criteria: a recording client can identify the origin of every model-bound
item and verify cancellation, limits, and failure disclosure with each policy.

## Phase 10 — In-memory orchestration

Status: planned.

Add typed nodes, edges, conditional routing, bounded fan-out and fan-in, and
explicit Agent handoff. Runs execute within one process and expose ordered
events, shared budgets, and caller-visible pending human input. Completion or
interruption returns data that an application may store, but Crystal does not
store sessions, checkpoint them, or resume a prior runtime instance.

Exit criteria: deterministic scenarios prove branch selection, result order,
budget accounting, cancellation, and no duplicate tool start within a live run.

## Future media lifecycles

Evaluate batch generation, generated-media streaming, resumable remote
generation, and realtime sessions as separate contracts when portable use cases
and lifecycle rules are established. Remote generation handles must not be
confused with Crystal session recovery.

Every phase requires a concrete use case, an ownership decision, and a
behavioral acceptance test before adding public contracts. No migration or
compatibility layer for a named framework is planned.
