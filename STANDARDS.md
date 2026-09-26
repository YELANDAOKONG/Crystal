# Crystal Engineering Standards

## General rules

- Prefer a small explicit public surface over convenience APIs with hidden
  behavior.
- Keep authoritative documents synchronized with material behavior changes.
- Runtime and exception text is plain English.
- Crystal-authored diagnostics exclude credentials, prompt text, reasoning text,
  opaque state, raw tool arguments, media bytes, media URIs, tool exception
  details, and stack traces.
- Public protocol values that can contain caller- or model-authored text use
  content-free default string representations.
- Comments explain constraints and intent rather than syntax.
- Do not leave commented-out production code.
- Do not modify dependencies without explicit authorization.

## Source organization

- Use file-scoped namespaces.
- Put exactly one type in each file and match the file name to the type name.
- Keep namespace ownership consistent with ARCHITECTURE.md.
- Keep assembly ownership and project references consistent with
  ARCHITECTURE.md.
- Order using directives with System first, third-party second, and Crystal
  namespaces third, separated when multiple groups exist.
- Do not use top-level statements.

## Project boundaries

- Crystal contains the provider-adapter protocol foundation and has no project
  references.
- Crystal.Tools references only Crystal.
- Crystal.Agents references Crystal and Crystal.Tools.
- Crystal.Harness references Crystal and Crystal.Agents.
- Crystal.Pipelines has no project references and remains usable for any typed
  asynchronous request/response or event stream.
- Crystal.Decorators references only Crystal and Crystal.Pipelines. It
  preserves each wrapped client's optional streaming interface and capability
  profile; unsupported streaming middleware is rejected.
- Production project references are one-way and contain no cycle.
- Text and multimodal model-facing tool protocol values remain in Crystal even
  though executable tool infrastructure belongs to Crystal.Tools.
- Shared build settings live in Directory.Build.props.
- CollectionSnapshot is shared as linked internal source. Do not make common
  implementation helpers public merely to cross an assembly boundary.

## C# conventions

- Use PascalCase for types and public members, camelCase for locals and
  parameters, and _camelCase for private fields.
- Prefix interfaces with I.
- Use C# keywords such as string and int instead of CLR type names.
- Use braces for every control-flow block.
- Every switch has an explicit fallback.
- Prefer immutable records for value data and classes for stateful behavior.
- Use collection expressions when they improve clarity.
- Use nameof for parameter and code-element references.
- Avoid magic numbers, double negatives, nested loops, and clever compression.
- Keep nullable reference types enabled and do not suppress warnings without a
  documented reason.
- Declare variables near first use and one variable per declaration.

## Async contracts

- Every externally implemented or I/O operation is asynchronous.
- Async methods end in Async.
- CancellationToken is the last parameter and is propagated unchanged or
  through an explicitly linked scope.
- Streams return IAsyncEnumerable<T> and use EnumeratorCancellation where
  implemented by async iterators.
- Never call Result, Wait, or GetAwaiter().GetResult().
- Library awaits use ConfigureAwait(false).
- Do not create unobserved background work.

## Public API rules

- Validate null, empty identifiers, indexes, limits, and structural invariants
  at construction boundaries.
- Argument exceptions include the parameter name.
- Snapshot mutable input collections and reject null elements.
- Do not expose internally mutable arrays, lists, media bytes, or JsonElement
  ownership.
- Preserve order unless the API explicitly declares otherwise.
- Text reasoning stream deltas identify candidate, item, and text-segment
  indexes. Multimodal streams identify candidate, item, message-content,
  reasoning-part, and tool-call-content indexes as applicable.
- Keep provider-originated values open rather than forcing lossy enums.
- Keep caller-supplied reasoning effort open as well: presets are optional,
  custom values pass through unchanged, and unsupported values fail at the
  external adapter boundary without implicit coercion.
- Use named policy types instead of ambiguous public booleans.
- Avoid ref and out except standard Try patterns.
- Do not expose provider responses, SDK types, transport types, or provider
  serialization attributes.
- Do not add a generic extension-data dictionary instead of a designed
  contract.
- Public APIs require XML documentation before a preview package.

## Protocol provenance

Model-bound text must be traceable to caller input, selected model output, or
caller-owned tool or policy output. Model-bound media must be exact caller input,
selected model output, or caller-owned multimodal tool or policy output.

Tests must prove:

- the first Agent model request contains exactly caller items;
- later requests add only selected model items and correlated tool results;
- reasoning classifications and opaque state remain order-for-order and
  byte-for-byte stable;
- multimodal content and generation inputs and outputs remain in exact order;
- Agent replay does not fetch, inspect, transcode, upload, download, or cache
  media;
- no error, retry, limit, selection, approval, or context behavior injects a
  message;
- event objects expose every configured transition; and
- streamed Agent events preserve each exact client event and assemble complete
  candidates before selection or tool execution; and
- an absent tool executor exposes no definitions.

## Error semantics

- Exceptions represent invalid use, provider failure, unhandled tool failure,
  cancellation, or a broken implementation.
- Expected finish reasons and configured Agent or Harness limit stops are data.
- Preserve an original exception as InnerException when Crystal wraps it.
- Never wrap OperationCanceledException as an ordinary failure.
- A tool failure becomes model-visible only when a caller-supplied mapper
  returns exact text or multimodal output.
- Unsupported provider semantics fail at the adapter boundary.

## Media and generation

- Content modalities and generation input purposes are closed portable values.
- MIME types, codecs, roles, finish reasons, and other provider-originated values
  remain open where lossless preservation requires it.
- Inline sources copy incoming bytes and never expose the owned array.
- URI sources require an absolute URI. Crystal stores but never resolves or
  downloads it.
- Source length and expiration preserve reported facts. Runtime code never
  refreshes an expiring source or infers an expiration.
- Replayable stream factories return a fresh readable stream at its beginning on
  every call; ownership transfers to the caller of OpenReadAsync.
- Image, audio, and video values require an explicit MIME type. Optional metadata
  reports known facts and is never inferred by runtime code.
- Capability profiles advertise individual input and output shapes. Do not build
  a provider constraint DSL into core contracts.
- Generation requirements, including requested output source shape, are hard.
  Adapters reject unsupported requirements or combinations rather than dropping,
  rewriting, or approximating them.
- Empty generation input, candidate, and item sequences remain valid portable
  structures. Adapters do not invent candidates or finish reasons.
- Image, audio, and video generation clients remain independent target-output
  contracts. Editing is expressed through typed source and mask inputs.
- Immediate single-request, batch, streaming, resumable-operation, and realtime
  APIs remain separate.
- Generated output preserves item and candidate order. Embedded video audio and a
  separate generated audio item are not interchangeable.
- Do not add a generic attachment, provider-option, billing-usage, or metadata
  dictionary.

## Agent and tool execution

- Text and multimodal Agent, Tool, and Harness public families remain independent.
- Candidate selection is caller-supplied.
- Tool execution mode and concurrency are explicit.
- Validate the complete tool batch against the immutable catalog before any
  text or multimodal tool invocation starts. Registration preflight does not
  imply transactional execution after invocation begins.
- Concurrent results preserve original call order.
- A tool batch is not partially started when its full size exceeds remaining
  Agent budget.
- Approval policies run before tool invocation.
- Rejection without caller-authored output terminates execution.
- Unhandled tool exceptions terminate execution.
- Model and tool attempts consume limits even when they fail or time out.
- Agent and Harness maximums are independently nullable: null means no
  configured limit, while finite values retain their current validation and
  accounting. An unlimited duration creates no timer, but cancellation still
  propagates.
- Agent usage is null unless every attempted model call reports usage.
- Context overflow is surfaced; Crystal does not truncate or summarize.
- A text Agent prefers IStreamingChatClient when available, forwards exact
  stream events, and assembles the complete response before candidate selection.
  Invalid or incomplete streams fail without synthesized model content.
- Multimodal Agent replay preserves media values and relies on caller-maintained
  URI and replayable-stream validity for the complete run.
- A multimodal Agent prefers IStreamingMultimodalChatClient when available,
  forwards exact provider events, and assembles the complete response before
  candidate selection. Invalid or incomplete streams fail without synthesized
  model content.

## Harness execution

- Sessions may have finite or unlimited shared depth, model-call, tool-call, and
  duration limits. Finite shared limits narrow unlimited invocation requests.
- Concurrent invocations reserve shared call capacity before they start.
- Successful invocations return unused reservation.
- Failed or abandoned invocations conservatively retain their reservation.
- Parent identifiers must refer to invocations registered in the same session.
- Routing and topology remain caller-owned.
- Session history storage, process-spanning recovery, and checkpointing belong
  to the consuming application. A new process receives caller-reconstructed
  inputs; Crystal does not restore a prior session.

## Pipeline execution

- Middleware is caller-owned and ordered. The first declared middleware is the
  outermost wrapper around the terminal operation.
- Construction snapshots middleware and rejects null entries or wrappers that
  return no operation.
- Pipelines do not modify requests, responses, streams, cancellation, or
  exceptions on their own.
- Any model-bound content emitted by middleware is caller-authored content and
  remains subject to protocol provenance requirements.
- Caller policies use the ordinary middleware delegates. A policy may forward,
  reject, or return exact caller-owned output; Crystal has no implicit
  exception-to-content disclosure or separate policy abstraction.
- Built-in observations contain no request, response, event, exception, or
  operation argument. Operation names are caller-supplied stable identifiers
  without sensitive content. Stream observations cover enumeration through
  disposal, including early abandonment.

## Dependency decision

The existing Newtonsoft.Json, Newtonsoft.Json.Bson, and System.Text.Json package
references are intentionally retained by the Crystal project. Higher-layer
projects add no package references. The current build may report that the
explicit System.Text.Json reference is unnecessary for net10.0; that warning is
accepted until the dependency decision changes.

## Verification

The required solution build is:

~~~bash
dotnet build Crystal.sln
~~~

The authorized xUnit project is Crystal.Tests. Run its relevant tests while
developing and the complete suite before handoff. Tests must cover externally
observable behavior and invariants, not merely duplicate implementation logic.
