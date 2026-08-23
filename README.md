# Crystal

Crystal is a provider-neutral C# library for text completion, chat, embeddings,
tool execution, bounded Agents, and explicit Agent Harness composition.

The current development line is text-only. Multimodal Chat, multimodal Agents,
and image, audio, and video generation are planned later as additive
capabilities.

## Design guarantees

- No built-in provider, authentication, transport, or model catalog.
- No built-in prompt or runtime-authored model text.
- No concrete tools.
- Ordered readable and opaque reasoning preservation.
- Explicit candidate, tool, approval, limit, and composition policies.
- Immutable public data contracts.

## Project status

Crystal targets net10.0 and has no compatibility baseline yet. The current
repository is establishing the complete text foundation. A test project has not
yet been authorized; the executable quality gate is:

~~~bash
dotnet build Crystal.sln
~~~

## Architecture

Start with:

- BUSINESS.md for product scope;
- ARCHITECTURE.md for ownership and runtime semantics;
- COMPATIBILITY.md for adapter requirements;
- STANDARDS.md for engineering rules; and
- ROADMAP.md for delivery status and deferred multimodal work.

## Provider adapters

A provider package implements only the capabilities it can preserve:

- IEmbeddingClient for text embeddings;
- ICompletionClient and optionally IStreamingCompletionClient;
- IChatClient and optionally IStreamingChatClient.

Provider configuration, model identifiers, wire options, DTOs, and exceptions
stay in that external package.

## Prompt provenance

Crystal never creates natural-language content for a model. During an Agent run,
the transcript contains only caller input, selected model output, and exact
caller-owned tool output. Limits and errors stop the run or throw; they do not
become hidden messages.

## Future media support

Future media work will introduce explicit typed capabilities. The text APIs will
remain text-only, and the project will not use a generic attachment bag as a
temporary compatibility shortcut.
