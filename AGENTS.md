# Agent Instructions

These instructions apply to the entire repository.

## Required reading

Read these documents before changing production code:

1. BUSINESS.md defines the product boundary and terminology.
2. ARCHITECTURE.md defines component ownership and runtime semantics.
3. COMPATIBILITY.md defines the external provider-adapter contract.
4. STANDARDS.md defines coding, API, dependency, and verification rules.
5. ROADMAP.md defines implementation order and current status.

When a design decision changes, update the relevant document in the same
change. Implementation must never become the only source of truth.

## Current product boundary

- Crystal is a reusable C#/.NET library, not an application or hosted service.
- The current release line supports text embedding, text completion, text chat,
  text tool results, Agent execution, and Harness composition.
- The current release line also supports explicit media sources, typed image,
  audio, and video values, non-streaming multimodal Chat, Tool, Agent, and
  Harness families, and immediate image, audio, and video generation clients.
- Text and multimodal client, Tool, Agent, and Harness contracts are independent.
  Existing text interfaces remain usable without accepting or returning media.
- The current release line contains no generic attachment or file-content bag,
  PDF contract, batch generation, generated-media streaming, resumable generation
  operation, or realtime media session.
- Future media lifecycles must remain additive and explicit. Do not add
  placeholders that reserve names without implemented portable semantics.

## Non-negotiable invariants

- Crystal contains no built-in natural-language prompts. It must not invent,
  prepend, append, repair, summarize, or rewrite model-bound text.
- Crystal contains no built-in model providers. Authentication, transport, wire
  DTOs, model identifiers, and SDK-specific types belong in external adapters.
- External adapters must preserve readable reasoning, opaque continuation
  state, ordering, and correlation without Crystal knowing the provider format.
- Crystal contains no concrete tools. Tool contracts, catalogs, approval,
  exception mapping, and dispatch are infrastructure; capabilities belong to
  callers and external packages.
- Agent and Harness behavior is explicit. Candidate selection, tool execution
  order, side-effect approval, exception disclosure, limits, routing, retries,
  and context reduction are caller-owned choices.
- Runtime-added data is limited to protocol envelopes, counters, events, and
  correlation metadata. Runtime code must not synthesize model-visible text.

The runtime may replay exact caller input, exact selected model output, and exact
registered-tool output. It may not produce natural-language content for a model.

## Change rules

- Do not add, remove, or update dependencies without explicit user approval.
- Keep the existing JSON package references until the user explicitly changes
  that decision.
- Do not add provider-, vendor-, transport-, or SDK-specific public types.
- Keep public data contracts immutable and nullable intent explicit.
- Preserve ordering for messages, completion items, candidates, tool calls,
  tool results, content blocks, generation inputs and outputs, reasoning items,
  opaque reasoning state, stream events, embedding inputs, and embedding
  outputs.
- Every asynchronous operation accepts a CancellationToken. Streaming uses
  IAsyncEnumerable<T>.
- Runtime messages are plain English and exclude secrets, prompt text, raw tool
  arguments, reasoning text, opaque state, media data, media URIs, and stack
  traces by default.
- Use one type per file, file-scoped namespaces, and STANDARDS.md conventions.
- Do not perform repository history operations unless explicitly requested.
- Ask before adopting a public business or architectural choice not covered by
  the authoritative documents.

## Verification

Run the narrowest relevant build while developing and the full available check
before handoff:

~~~bash
dotnet build Crystal.sln
~~~

No test project is currently authorized. Do not claim automated test coverage,
and do not run dotnet test as though a test suite exists.
