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

## Phase 0 — Product and architecture reset

Status: complete.

Deliverables:

- one vocabulary for Completion, Chat, Tool, Agent, and Harness;
- explicit current exclusions and future modality direction;
- provider-adapter reasoning requirements; and
- executable design rules for prompt neutrality and bounded execution.

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
- explicit serial and bounded-concurrent execution;
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
- bounded Harness sessions;
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

Status: deferred pending authorization.

Future decisions:

- select and add a test framework;
- add unit, protocol, Agent, and Harness contract tests;
- add API compatibility tooling;
- decide package metadata and CI;
- review whether all current JSON dependencies remain necessary; and
- establish the first public compatibility baseline.

Until then, dotnet build Crystal.sln is the executable verification.

## Phase 6 — Multimodal and media generation

Status: initial non-streaming and immediate-generation scope complete.

Completed deliverables:

1. explicit inline-copy, absolute-URI, and replayable-stream media semantics,
   including optional source expiration;
2. typed image, audio, and video values with explicit MIME and known metadata;
3. closed typed multimodal content and coarse input/output capability profiles;
4. independent non-streaming multimodal Chat and Tool protocol contracts;
5. independent executable multimodal Tool, Agent, and Harness families;
6. independent immediate image, audio, and video generation clients;
7. ordered text, image, audio, video, and reasoning generation output;
8. typed source, reference, mask, first-frame, last-frame, and audio-for-video
   inputs; and
9. portable hard output requirements with adapter-owned rejection semantics.

Deferred Phase 6 lifecycles:

1. explicit batch-generation submission and result semantics;
2. modality-specific generated-media streaming and preview semantics;
3. resumable long-running operation handles, polling, persistence, and explicit
   remote cancellation semantics;
4. stateful realtime audio and video sessions; and
5. automated protocol and runtime tests after a test project is authorized.

No Phase 6 production type may be a placeholder media abstraction, generic
option bag, provider resource handle, or universal edit mode.
