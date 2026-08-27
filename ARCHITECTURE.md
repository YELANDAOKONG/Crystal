# Crystal Architecture

## Status

This document is the authoritative architecture for the text-first development
line. There is no compatibility baseline yet, so public names may change while
the design documents and implementation change together.

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

Owns cross-capability primitives, Reasoning, Embeddings, Completions, Chat, and
the model-facing tool protocol values required by Chat: ToolDefinition,
ToolCall, ToolResult, and ToolResultStatus. Keeping those four values in Crystal
lets provider adapters represent complete Chat protocol traffic without taking
a dependency on executable tool infrastructure.

### Crystal.Tools

Owns executable tool contracts, catalogs, scheduling, policies, exception
mapping, and dispatch. It references Crystal.

### Crystal.Agents

Owns Agent contracts, events, limits, results, and runtime execution. It
references Crystal and Crystal.Tools.

### Crystal.Harness

Owns Harness contracts, events, reservations, sessions, and explicit Agent
composition. It references Crystal and Crystal.Agents.

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

Future multimodal chat will use an explicit capability contract. It will not
silently change IChatClient into a media client.

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
Crystal assembly. The remaining types in this namespace are compiled into the
Crystal.Tools assembly.

The standard executor preserves input call order in its returned results even
when calls run concurrently. Unknown tools, rejected calls without a
caller-authored result, and unhandled tool exceptions terminate execution. The
runtime never writes an error message for the model.

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

Provider streaming uses typed IAsyncEnumerable<T> events. Candidate and item
indexes preserve interleaving. Reasoning text deltas additionally carry a
text-segment index; every delta for one semantic segment uses the same index.
Identifier, name, argument, text, and reasoning deltas are explicitly identified
as deltas; adapters must not pretend partial data is complete.

A complete stream must be aggregatable into the same semantic response as the
non-streaming operation. Opaque state may be buffered by an adapter and emitted
as a completed state event.

Agent and Harness streams end with a typed completion event containing the same
result returned by their non-streaming methods. Consumer cancellation stops
enumeration and propagates to in-flight model, policy, and tool operations.

## Safety and disclosure

- No mutable global registries or ambient service locators.
- No automatic retry, prompt repair, candidate heuristic, context reduction, or
  tool exception disclosure.
- No partial tool batch execution caused by a remaining-budget calculation.
- Concurrent tool execution requires an explicit bounded concurrency setting.
- Reasoning text, opaque state, raw tool arguments, prompts, and tool exception
  details are absent from Crystal-authored diagnostics.
- Policies that create model-visible text are supplied by the caller and their
  returned text is replayed exactly.

## Future multimodal boundary

The future media architecture must be additive:

- existing text contracts remain valid and text-only;
- new capability interfaces advertise multimodal support explicitly;
- typed image, audio, and video values replace an untyped attachment bag;
- large-media ownership and lifetime are designed before binary APIs ship; and
- generation lifecycles are designed per modality instead of assuming that
  image, audio, and video operations have identical behavior.

No current production type is a placeholder media abstraction.

## Dependency and serialization boundary

The Crystal project retains its existing Newtonsoft.Json, Newtonsoft.Json.Bson,
and System.Text.Json references by explicit user decision. The higher-layer
projects add only project references. Domain contracts carry no provider
serialization attributes. Tool schemas use System.Text.Json JsonElement, and
model-generated tool arguments remain raw text until a tool chooses to parse
them.
