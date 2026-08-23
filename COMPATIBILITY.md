# External Provider Compatibility

## Purpose

Crystal ships no provider implementation. This document records the portable
semantics an external adapter must preserve across current text-model protocols.
Named services are compatibility evidence only; their names and SDK types must
not enter Crystal public contracts.

The evidence was reviewed against official provider documentation on
2026-08-23.

## Common capability rule

An adapter implements only the interfaces it can honor. Streaming is optional
and separate from non-streaming. A service may implement Chat without
Completion or Embeddings. An adapter must reject unsupported input, options, or
output shapes instead of silently dropping, rewriting, or emulating them.

Provider and model selection belongs to configured adapter instances. Core
requests contain no provider model identifier.

## Reasoning interoperability

Current provider protocols expose materially different reasoning forms:

| Protocol evidence | Readable surface | Continuation surface |
|---|---|---|
| OpenAI Responses | Optional summaries | Encrypted reasoning items can be included and replayed for stateless multi-turn use |
| Anthropic Messages | Summarized, empty, or omitted thinking text | Complete thinking, signature, and redacted blocks must accompany tool-result continuations unchanged |
| Google Gemini | Optional chronological thought summaries | Encrypted thought signatures are first-class state and summaries may be absent |
| DeepSeek Chat | Plain reasoning_content | Tool workflows require reasoning_content to be passed back fully on subsequent requests |
| xAI Responses | Summarized reasoning on supported models | Encrypted reasoning content can be returned and supplied to later conversation calls |
| OpenRouter | Plain, summarized, or encrypted reasoning details | Consecutive reasoning_details blocks must retain their full structure and sequence |

Official evidence:

- [OpenAI Responses API](https://developers.openai.com/api/reference/cli/resources/responses/methods/create)
- [Anthropic thinking tool workflows](https://platform.claude.com/docs/en/build-with-claude/thinking-tool-workflows)
- [Google Gemini thinking](https://ai.google.dev/gemini-api/docs/thinking)
- [DeepSeek thinking mode](https://api-docs.deepseek.com/guides/thinking_mode/)
- [xAI reasoning](https://docs.x.ai/developers/model-capabilities/text/reasoning)
- [OpenRouter reasoning tokens](https://openrouter.ai/docs/guides/best-practices/reasoning-tokens)

## Crystal reasoning representation

One provider-native reasoning block maps to one ordered ReasoningContent value:

- TextSegments contains zero or more readable summaries or traces.
- State contains an optional complete provider-native continuation encoding.
- At least readable text or opaque state is present.

Opaque state can contain a serialized provider block rather than only one wire
field. This allows an adapter to retain identifiers, encrypted content,
signatures, redacted variants, and format metadata without leaking those
details into Crystal.

The adapter chooses its own stable Format value and consumes only formats it
recognizes. Crystal copies state and replays it byte-for-byte.

## Required adapter behavior

An adapter must:

1. preserve every supported text, reasoning, tool-call, and tool-result item in
   provider order;
2. preserve each reasoning block separately instead of merging blocks;
3. retain the complete provider representation needed for continuation;
4. distinguish readable summaries from readable raw traces;
5. support reasoning blocks with no readable text;
6. reject opaque formats it does not understand;
7. preserve raw model tool arguments until the execution boundary;
8. keep multiple candidates and their ordering;
9. normalize usage without inferring hidden token counts from visible text;
10. keep provider DTOs and raw responses outside the portable result; and
11. ensure streaming aggregation is semantically equivalent to non-streaming.

## Current text-only limitation

Some provider protocols can surface non-text reasoning summaries or other media
blocks. The current Crystal text profile cannot represent those readable media
surfaces. An adapter may use opaque state for continuation data, but it must not
claim lossless readable-output support if a provider response contains media
that Crystal cannot expose. It must select a compatible provider mode or reject
the unsupported response.

This limitation will be addressed by a future explicit multimodal capability,
not by smuggling binary data through text or opaque reasoning state.

## Tool protocol compatibility

- Tool definitions are caller-authored names, descriptions, and JSON schemas.
- Tool call identifiers and names are complete values in non-streaming output.
- Model-generated arguments are raw strings because partial or invalid JSON is
  a valid intermediate model outcome.
- Streaming identifier, name, and argument fields are deltas and preserve their
  arrival order.
- Tool results are textual and correlated by the exact model call identifier.
- Provider-native built-in tools are adapter features, not Crystal tools, unless
  an external package explicitly adapts them to caller-visible contracts.

## Embedding compatibility

Embedding inputs and outputs are ordered. An adapter must return one vector per
accepted input in the same order. It must not silently omit rejected inputs.
Dimensions are reported by each vector and are not hard-coded by Crystal.

Only text embeddings are in the current profile.
