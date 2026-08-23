# Crystal Product Definition

## Mission

Crystal is a provider-neutral, prompt-neutral, and tool-neutral C# library for
integrating text models and building inspectable Agents and Agent Harnesses. It
defines stable protocol contracts and deterministic execution infrastructure
while leaving models, transport, prompts, tools, policies, and application state
to external code.

Crystal is a library. It does not host an application, expose a service, select
a model, or own an end-user experience.

## Intended users

- Provider-adapter authors implementing portable text-model contracts.
- Application developers using completion, chat, or embedding capabilities.
- Tool authors exposing caller-owned capabilities to an Agent.
- Agent authors composing model calls and tools under explicit policies.
- Harness authors coordinating parent and child Agents without a built-in
  routing topology.

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

### Reasoning

- Provider-neutral request hints for mode, effort, visible output, and budget.
- Readable reasoning text classified as summary or trace.
- Opaque continuation state copied and replayed unchanged.
- Ordered preservation across ordinary and tool-calling turns.

### Tools

- Caller-authored definitions and JSON input schemas.
- Raw model-generated argument text.
- Immutable catalogs and explicit serial or concurrent dispatch.
- Optional caller-owned approval and exception-to-output policies.
- Textual outputs correlated to model tool calls.

### Agent

- A prompt-free model/tool loop.
- Required finite model-call, tool-call, and duration limits.
- Caller-supplied candidate selection.
- Typed events containing exact model requests, responses, and tool results.
- Exact transcript preservation and explicit stop reasons.

### Harness

- Named Agent registration.
- Explicit parent-child invocation.
- Shared depth, model-call, tool-call, duration, and cancellation boundaries.
- Invocation ancestry and event forwarding.
- No built-in router, supervisor prompt, graph, or persistence store.

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

- multimodal chat or multimodal Agents;
- image, audio, or video input or output;
- image, audio, or video generation;
- binary attachments, PDFs, or general file content;
- built-in providers, authentication, transport, or model catalogs;
- built-in prompts, personas, templates, repair messages, or summaries;
- built-in search, filesystem, shell, clock, network, or other concrete tools;
- retrieval stores, memory stores, persistence, hosting, UI, or telemetry
  backends;
- automatic retries, context truncation, routing, planning, or side-effect
  approval.

## Future modality direction

Multimodal and media generation support is planned after the text foundation is
usable. The compatibility promise is architectural:

- text-only interfaces remain text-only;
- future modalities use explicit contracts and capability interfaces;
- image support arrives before audio and video where practical;
- audio and video generation and multimodal use remain first-class future
  capabilities rather than being forced through image or file abstractions; and
- no current API accepts an untyped media bag merely to reserve a name.

This direction reserves namespace and dependency boundaries, not placeholder
types or behavior.

## Acceptance criteria for the text foundation

The text foundation is usable when a developer can:

1. implement completion, chat, and embedding adapters without Agent internals;
2. preserve every supported reasoning block and opaque continuation payload;
3. register and execute caller-defined tools without Crystal-authored text;
4. run a bounded tool-calling Agent and inspect every protocol transition;
5. distinguish normal completion from every configured limit stop;
6. compose named parent and child Agents under shared Harness budgets;
7. cancel model, tool, Agent, and Harness work cooperatively; and
8. consume the production package without receiving a provider, prompt, or
   concrete tool.
