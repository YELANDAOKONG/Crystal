# External Provider Compatibility

## Purpose

Crystal ships no provider implementation. This document records the portable
semantics an external adapter must preserve across current text, multimodal, and
immediate-generation protocols.
Named services are compatibility evidence only; their names and SDK types must
not enter Crystal public contracts.

An external provider adapter needs only the Crystal assembly. Model-facing tool
definitions, calls, and results remain in that assembly so an adapter can
preserve complete Chat traffic without referencing Crystal.Tools,
Crystal.Agents, or Crystal.Harness.

The text and reasoning evidence was reviewed against official provider
documentation on 2026-08-23. Multimodal and generation evidence was reviewed on
2026-08-30.

## Common capability rule

An adapter implements only the interfaces it can honor. Streaming is optional
and separate from non-streaming. A service may implement Chat without
Completion or Embeddings. An adapter must reject unsupported input, options, or
output shapes instead of silently dropping, rewriting, or emulating them.

Provider and model selection belongs to configured adapter instances. Core
requests contain no provider model identifier. Capability profiles describe
portable individual input and output shapes; they are not a conditional-rule or
model-constraint language. Adapters validate model-specific combinations,
cardinality, size, duration, and other constraints.

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

## Multimodal and generation evidence

Official provider contracts demonstrate why Crystal keeps typed inputs broad but
lifecycles separate:

- [Google Gemini video generation](https://ai.google.dev/gemini-api/docs/video)
  documents video generation conditioned by text, images, video, and audio.
- [Runway input constraints](https://docs.dev.runwayml.com/assets/inputs/) show
  that valid input combinations and counts vary by model and task.
- [Google Gemini image generation](https://ai.google.dev/gemini-api/docs/image-generation)
  documents multimodal inputs, including video, and interleaved text and image
  output.
- [Amazon Bedrock inference APIs](https://docs.aws.amazon.com/bedrock/latest/userguide/inference-api.html)
  expose model input modalities, output modalities, and streaming support as
  distinct capabilities.
- [OpenAI image generation](https://developers.openai.com/api/reference/resources/images)
  distinguishes partial preview images from final generated images.
- [Google Gemini Live](https://ai.google.dev/api/live) defines a stateful realtime
  session rather than an ordinary request/response operation.
- [Vertex AI video generation](https://docs.cloud.google.com/vertex-ai/generative-ai/docs/video/generate-videos-from-first-and-last-frames)
  uses long-running operations for video generation.

These contracts are evidence for portable semantics, not provider types or model
identifiers in Crystal.

## Crystal reasoning representation

One provider-native text reasoning block maps to one ordered ReasoningContent
value:

- TextSegments contains zero or more readable summaries or traces.
- State contains an optional complete provider-native continuation encoding.
- At least readable text or opaque state is present.

A multimodal reasoning block maps to MultimodalReasoningContent. Each ordered
readable part uses the same closed typed content family as
multimodal messages and retains an open summary, trace, or provider-originated
classification. The block can also carry the same opaque continuation state.

Opaque state can contain a serialized provider block rather than only one wire
field. This allows an adapter to retain identifiers, encrypted content,
signatures, redacted variants, and format metadata without leaking those
details into Crystal.

The adapter chooses its own stable Format value and consumes only formats it
recognizes. Crystal copies state and replays it byte-for-byte.

## Required adapter behavior

An adapter must:

1. preserve every supported text, media, reasoning, tool-call, and tool-result
   item in provider order;
2. preserve each reasoning block separately instead of merging blocks;
3. assign stable zero-based text-segment indexes to streamed reasoning text,
   with every delta for one semantic segment sharing its index;
4. retain the complete provider representation needed for continuation;
5. distinguish readable summaries from readable raw traces;
6. support reasoning blocks with no readable text;
7. reject opaque formats it does not understand;
8. preserve raw model tool arguments until the execution boundary;
9. keep multiple candidates and their ordering;
10. normalize usage without inferring hidden token counts from visible text;
11. keep provider DTOs and raw responses outside the portable result; and
12. ensure streaming aggregation is semantically equivalent to non-streaming;
13. preserve explicit media MIME types and source shapes without implicit fetch or
    conversion;
14. advertise only portable input and output shapes it can honor;
15. reject unsupported generation requirements and conditional combinations; and
16. keep provider billing fields, media handles, model identifiers, and options
    outside portable results.

## Text and multimodal profiles

IChatClient remains a text profile. An adapter implementing it must select a
provider mode whose readable output can be represented by text Chat items or
reject an unsupported media response. Binary data must never be smuggled through
text or opaque reasoning state.

IMultimodalChatClient is a separate profile. It can preserve readable text, image,
audio, and video reasoning content plus opaque continuation state. An adapter
must advertise the individual input and output modalities and media source shapes
it supports. It must still validate provider-specific role, combination, and
cardinality rules for each request.

## Media source compatibility

- Every image, audio, and video value has an explicit MIME type. Adapters must not
  infer a different representation and silently rewrite the value.
- InlineMediaSource contains a complete immutable copy.
- UriMediaSource is an absolute caller value. Crystal never fetches it; an adapter
  may accept it only when its documented behavior can honor that source shape.
- ReplayableStreamMediaSource transfers ownership of each opened stream to the
  consumer. Each call must return a fresh readable stream at its beginning.
- Length and ExpiresAt preserve reported source facts. Crystal does not refresh,
  resolve, or fetch an expiring source.
- File paths and provider media identifiers remain external-adapter concerns.
- Adapter errors and diagnostics must not include media bytes or media URIs by
  default.

## Immediate generation compatibility

- IImageGenerationClient, IAudioGenerationClient, and IVideoGenerationClient are
  target-output contracts, not aliases for one universal media generator.
- Ordered typed inputs may include text, image, audio, and video whenever the
  client advertises the corresponding modality, purpose, and source shape. In
  particular, video generation can advertise audio reference or source input.
- Source and mask inputs express editing or transformation. Adapters must not
  invent a generation mode or rewrite inputs to emulate an unsupported edit API.
- Output source shape, MIME, dimensions, aspect ratio, duration, frame rate,
  channel count, sample rate, codec, and embedded-audio requirements are hard
  when present. Unsupported
  values or combinations must be rejected.
- RequestedCandidateCount asks the provider for a positive count; it does not
  require an adapter to invent candidates when the provider returns no result.
  An adapter that cannot request a count must reject the option.
- Generated candidate items preserve provider order across text, image, audio,
  video, and reasoning. Candidate lists and item lists may be empty when the
  provider returns no result. FinishReason is populated only when the provider
  reports one. Adapters must not invent candidates or finish reasons.
- A separate audio output remains a separate item. A video value reports whether
  its own representation contains embedded audio.
- TokenUsage is present only when provider-reported token accounting is
  semantically available. Provider billing units do not belong in a generic
  usage dictionary.
- Immediate clients return complete responses. Batch submission, partial
  previews, resumable remote handles, remote cancellation, and realtime sessions
  are not represented by the immediate interfaces. CancellationToken cancels
  local cooperative work; it does not imply cancellation of an already submitted persistent provider job.

## Tool protocol compatibility

- Tool definitions are caller-authored names, descriptions, and JSON schemas.
- Tool call identifiers and names are complete values in non-streaming output.
- Model-generated arguments are raw strings because partial or invalid JSON is
  a valid intermediate model outcome.
- Streaming identifier, name, and argument fields are deltas and preserve their
  arrival order.
- Text tool results are textual and correlated by the exact model call
  identifier. Multimodal tool calls preserve exact raw argument text plus
  optional ordered typed content; results preserve ordered typed content and use
  their independent correlation contract.
- Provider-native built-in tools are adapter features, not Crystal tools, unless
  an external package explicitly adapts them to caller-visible contracts.

## Embedding compatibility

Embedding inputs and outputs are ordered. An adapter must return one vector per
accepted input in the same order. It must not silently omit rejected inputs.
Dimensions are reported by each vector and are not hard-coded by Crystal.

Only text embeddings are in the current profile.
