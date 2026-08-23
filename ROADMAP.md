# Crystal Roadmap

## Current direction

Crystal is a new net10.0 library with no compatibility baseline. The current
development line deliberately focuses on usable text-model integration before
multimodal or media generation work.

Large public API changes are allowed while this roadmap reports no preview
baseline. Design documents and code must change together.

## Recorded decisions

- 2026-08-22: Crystal has no built-in prompts, providers, or concrete tools.
- 2026-08-22: Reasoning is ordered protocol data with readable and opaque
  surfaces.
- 2026-08-23: Breaking changes are allowed because no consumer depends on the
  project.
- 2026-08-23: Current work is text-only.
- 2026-08-23: Multimodal Chat, multimodal Agents, and image generation are
  deferred.
- 2026-08-23: Image, audio, and video capabilities remain committed future
  directions and must be added through explicit additive interfaces.
- 2026-08-23: Existing JSON package references remain unchanged.
- 2026-08-23: No unit-test project is currently authorized.
- 2026-08-23: The existing net10.0 target remains unchanged.

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
- non-streaming and optional typed streaming client interfaces; and
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

Status: deliberately deferred.

Design order:

1. define media ownership, lifetime, MIME, URI, and inline-data semantics;
2. add explicit multimodal Chat capability without changing text IChatClient;
3. add explicit multimodal Agent capability;
4. add image generation and editing lifecycles;
5. extend multimodal and generation contracts for audio; and
6. extend them for video and long-running generation.

No Phase 6 placeholder type belongs in the current production assembly.
