# Crystal Engineering Standards

## General rules

- Prefer a small explicit public surface over convenience APIs with hidden
  behavior.
- Keep authoritative documents synchronized with material behavior changes.
- Runtime and exception text is plain English.
- Crystal-authored diagnostics exclude credentials, prompt text, reasoning text,
  opaque state, raw tool arguments, tool exception details, and stack traces.
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
- Production project references are one-way and contain no cycle.
- ToolDefinition, ToolCall, ToolResult, and ToolResultStatus remain in the
  Crystal assembly even though their namespace is Crystal.Tools.
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
- Do not expose internally mutable arrays, lists, or JsonElement ownership.
- Preserve order unless the API explicitly declares otherwise.
- Reasoning stream deltas identify candidate, item, and text-segment indexes.
- Keep provider-originated values open rather than forcing lossy enums.
- Use named policy types instead of ambiguous public booleans.
- Avoid ref and out except standard Try patterns.
- Do not expose provider responses, SDK types, transport types, or provider
  serialization attributes.
- Do not add a generic extension-data dictionary instead of a designed
  contract.
- Public APIs require XML documentation before a preview package.

## Protocol provenance

Model-bound text must be traceable to caller input, selected model output, or
caller-owned tool or policy output.

Tests, once authorized, must prove:

- the first Agent model request contains exactly caller items;
- later requests add only selected model items and correlated tool results;
- reasoning and opaque state remain order-for-order and byte-for-byte stable;
- no error, retry, limit, selection, approval, or context behavior injects a
  message;
- event objects expose every configured transition; and
- an absent tool executor exposes no definitions.

## Error semantics

- Exceptions represent invalid use, provider failure, unhandled tool failure,
  cancellation, or a broken implementation.
- Expected finish reasons and configured Agent or Harness limit stops are data.
- Preserve an original exception as InnerException when Crystal wraps it.
- Never wrap OperationCanceledException as an ordinary failure.
- A tool failure becomes model-visible only when a caller-supplied mapper
  returns exact ToolOutput text.
- Unsupported provider semantics fail at the adapter boundary.

## Agent and tool execution

- Candidate selection is caller-supplied.
- Tool execution mode and concurrency are explicit.
- Concurrent results preserve original call order.
- A tool batch is not partially started when its full size exceeds remaining
  Agent budget.
- Approval policies run before tool invocation.
- Rejection without caller-authored output terminates execution.
- Unhandled tool exceptions terminate execution.
- Model and tool attempts consume limits even when they fail or time out.
- Agent usage is null unless every attempted model call reports usage.
- Context overflow is surfaced; Crystal does not truncate or summarize.

## Harness execution

- Sessions have finite shared depth, model-call, tool-call, and duration limits.
- Concurrent invocations reserve shared call capacity before they start.
- Successful invocations return unused reservation.
- Failed or abandoned invocations conservatively retain their reservation.
- Parent identifiers must refer to invocations registered in the same session.
- Routing and topology remain caller-owned.

## Dependency decision

The existing Newtonsoft.Json, Newtonsoft.Json.Bson, and System.Text.Json package
references are intentionally retained by the Crystal project. Higher-layer
projects add no package references. The current build may report that the
explicit System.Text.Json reference is unnecessary for net10.0; that warning is
accepted until the dependency decision changes.

## Verification

The currently authorized executable check is:

~~~bash
dotnet build Crystal.sln
~~~

There is no authorized test project. Report that limitation explicitly.
