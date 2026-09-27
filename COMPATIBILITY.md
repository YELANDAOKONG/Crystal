# External Provider Compatibility

## Purpose

Crystal ships no provider implementation. This document records the portable
semantics an external adapter must preserve across current text, multimodal,
generation, and realtime protocols.
The compatibility target is provider behavior and portable data semantics;
Crystal does not implement API compatibility or migration for another .NET
library.
Named services are compatibility evidence only; their names and SDK types must
not enter Crystal public contracts.

An external provider adapter needs only the Crystal assembly. Model-facing tool
definitions, calls, and results remain in that assembly so an adapter can
preserve complete Chat traffic without referencing Crystal.Tools,
Crystal.Agents, or Crystal.Harness.

The text and reasoning evidence was reviewed against official provider
documentation on 2026-08-23, with model-dependent effort controls checked on
2026-09-27. Multimodal and generation evidence was reviewed on 2026-08-30,
with multimodal Chat streaming reviewed on 2026-09-22 and remote operation and
realtime lifecycle evidence checked on 2026-09-27.

## Common capability rule

An adapter implements only the interfaces it can honor. Streaming is optional
and separate from non-streaming. A service may implement Chat without
Completion or Embeddings. An adapter must reject unsupported input, options, or
output shapes instead of silently dropping, rewriting, or emulating them.

Reasoning-effort support varies by configured model. Crystal's named effort
values are examples, not a closed list or a promise that any model accepts them.
An adapter receives the exact caller-supplied ReasoningEffort.Value, including
values outside the presets. It must map a value it supports or reject the
request; it must not silently clamp or substitute another effort. Mode and
token-budget hints are separate controls and cannot be used as an implicit
fallback for an unsupported effort.

The [OpenAI reasoning guide](https://developers.openai.com/api/docs/guides/reasoning),
[Gemini thinking guide](https://ai.google.dev/gemini-api/docs/thinking), and
[Claude effort guide](https://platform.claude.com/docs/en/build-with-claude/effort)
document differing levels, defaults, and controls. These are evidence for an
open effort contract, not values to embed as provider-specific Crystal presets.

Provider and model selection belongs to configured adapter instances. Core
requests contain no provider model identifier. Capability profiles describe
portable individual input and output shapes; they are not a conditional-rule or
model-constraint language. Adapters validate model-specific combinations,
cardinality, size, duration, and other constraints.

## Structured final text requirement

JsonOutputRequirement is a caller-authored JSON Schema and a hard output
constraint on Completion and text or multimodal Chat. Its root is a JSON object
or boolean; Crystal does not interpret its keywords or choose a dialect. For
each normally completed candidate (FinishReason.Stop) without tool calls,
concatenate its user-visible text in
item and content order, excluding reasoning. CompletionText, assistant
ChatMessage.Text, and TextContent in assistant multimodal messages are the
respective text surfaces. The result must parse as one JSON value that conforms
to the supplied schema; a normal final candidate cannot include non-text media.
Intermediate stream deltas need not parse, but their assembled normal final
candidate must meet the same rule. Tool-call or other nonterminal candidates
are not rewritten into JSON; the requirement remains on subsequent Agent turns.

An adapter must reject the request when it cannot support the schema or combine
the requirement with requested tools or modalities. If a provider returns a
normal final candidate that violates the accepted requirement, the adapter
reports failure rather than silently returning it. Crystal does not validate,
repair, or add a formatting prompt. ToolDefinition.InputSchema governs tool
arguments separately and does not imply strict tool-call enforcement. It also
accepts an object or boolean JSON Schema root; Crystal preserves it without
interpreting keywords or parsing model-generated argument text.

The [OpenAI structured-output guide](https://developers.openai.com/api/docs/guides/structured-outputs),
[Claude structured-output guide](https://platform.claude.com/docs/en/build-with-claude/structured-outputs),
and [Gemini structured-output guide](https://ai.google.dev/gemini-api/docs/structured-output)
show distinct final-output and tool-input controls, with model-dependent
support. These are adapter mapping evidence, not provider fields in Crystal.

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
- [Gemini Live capabilities](https://ai.google.dev/gemini-api/docs/live-api/capabilities)
  document bidirectional text, audio, and video segments, automatic or explicit
  activity boundaries, and tool responses.
- [Vertex AI video generation](https://docs.cloud.google.com/vertex-ai/generative-ai/docs/video/generate-videos-from-first-and-last-frames)
  uses long-running operations for video generation.
- [Google Veo generation](https://ai.google.dev/gemini-api/docs/veo) describes
  submission and later polling of a long-running operation.

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
    outside portable results;
17. assign stable zero-based indexes to multimodal message content, reasoning
    parts, and tool-call content throughout a stream; and
18. emit media content as complete typed content rather than media-byte deltas.

## Text and multimodal profiles

IChatClient remains a text profile. An adapter implementing it must select a
provider mode whose readable output can be represented by text Chat items or
reject an unsupported media response. Binary data must never be smuggled through
text or opaque reasoning state.

IStreamingChatClient adds optional typed text streaming. A text Agent consumes
that stream when available, forwards each exact event, and assembles complete
candidates before selection. Candidate, item, and reasoning text-segment
indexes must remain stable and contiguous so the stream can be assembled
without inferred content.

IMultimodalChatClient is a separate profile. It can preserve readable text,
image, audio, and video reasoning content plus opaque continuation state.
IStreamingMultimodalChatClient adds optional typed streaming without changing
that complete-response contract. An adapter must advertise the individual input
and output modalities and media source shapes it supports. It must still validate
provider-specific role, combination, and cardinality rules for each request.

A multimodal stream starts each message explicitly so its role and an empty
content sequence remain representable. Text content may be emitted as deltas.
Complete content events carry one exact TextContent, ImageContent, AudioContent,
or VideoContent value and cannot be mixed with text deltas at the same index.
Reasoning parts use the same rule and retain their classification. Tool-call
identifier, name, and raw arguments remain deltas, while optional tool-call
content arrives as complete indexed content blocks.

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
- An adapter may implement both immediate and streaming generation interfaces.
  The immediate Crystal.Decorators wrapper preserves the stream interface in
  that case and applies separate caller-owned middleware to each path. If the
  two interfaces report different capability objects, the wrapper exposes the
  exact object reported by each interface. Stream middleware supplied for an
  immediate-only client is rejected.

## Remote generation operation compatibility

- An adapter may implement an image, audio, or video operation client only when
  it can submit a request and subsequently poll its status using a returned
  ticket. Implementing an immediate client does not imply operation support.
- The ticket's format and bytes are adapter-owned. A poll may return a new
  ticket, which supersedes the prior one. The caller may retain and resupply it
  to a compatible adapter instance; Crystal provides no ticket store.
- The adapter maps remote state to Pending, Running, Completed, Failed, or
  Canceled without inventing a completed response. Completed includes the
  same exact ordered target-specific response shape as immediate generation;
  other states carry no response.
- An unrecognized or expired ticket is an adapter failure. Poll must not
  submit a new operation as a fallback. A remote Failed state is distinct from
  a transport or adapter exception.
- CancellationToken stops local cooperative waiting. After acceptance, it
  does not assert remote cancellation. Remote cancellation, poll timing, and
  retries remain outside the portable operation interfaces.
- Typed Crystal.Decorators operation wrappers compose separate caller-owned
  middleware for StartAsync and PollAsync. They preserve exact requests,
  tickets, snapshots, and cancellation tokens unless middleware changes them.
  They retain no ticket or polling schedule.

## Generation batch compatibility

- Optional image, audio, and video batch clients submit an ordered non-empty
  set of exact target-specific requests as one remote operation. Adapters reject
  unsupported batch sizes or combinations; they do not silently fan out calls
  through immediate clients.
- A completed batch response returns exactly one terminal item for each input
  in input order. Completed items contain exact generation responses; Failed
  and Canceled items contain none. An entire remote batch can instead end with
  an outer Failed or Canceled status.
- Batch start and poll use the same opaque-ticket and local-cancellation
  semantics as single remote generation operations. Poll never submits a new
  batch as a fallback.
- Typed Crystal.Decorators batch wrappers compose separate submission and poll
  middleware without retaining a ticket or submitted request collection.
- Consumers may opt into GenerationBatchValidation at submission or polling to
  reject a completed response with an unexpected input count. Poll validation
  requires the caller-retained submitted count and stores no operation state.

## Generated-media stream compatibility

- Optional image, audio, and video stream clients are independent from their
  immediate and remote-operation clients. Adapters implement only the stream
  modes they can honor.
- Complete previews contain a typed image, audio, or video value. Encoded chunks
  preserve exact bytes, MIME, modality, and candidate/item/revision indexes. A
  single revision uses one complete preview or contiguous chunks beginning at
  index zero and ending with one final chunk. Different items may interleave.
- A completed chunk revision is provisional. Later revisions supersede it; the
  complete generation response in the stream's single terminal event is
  authoritative and preserves candidate and item order.
- A failed or canceled enumeration has no completed event. CancellationToken
  applies to enumeration and cooperative provider work; it does not imply
  remote cancellation of an accepted persistent job.
- The independent streaming generation clients can be wrapped by typed
  Crystal.Decorators clients. Wrappers preserve the exact declared capability
  object, request, cancellation token, and event stream unless caller-owned
  middleware explicitly changes them.
- Consumers may opt into GenerationStreamValidation middleware to reject
  malformed revision and chunk order or a missing, repeated, or nonterminal
  completion event. This validation forwards exact events and does not repair
  an adapter stream or construct final media.

## Realtime media session compatibility

- A realtime adapter implements IRealtimeMediaClient only if it can open a live
  bidirectional session. It advertises individual input/output shapes and
  supported turn modes; unsupported output modalities, initial context, tools,
  reasoning hints, or combinations fail at OpenAsync.
- SendAsync preserves caller input order and exact typed text or independently
  decodable media segments. An explicit end-of-turn event is used only in an
  explicit turn-mode session. The caller serializes sends; receiving may
  overlap. No adapter invents prompts or transcript text.
- ReceiveAsync forwards exact output segments in observed order. Output IDs
  remain stable within a session, segment indexes start at zero per output and
  remain ordered, and completion closes that output with its exact reported
  finish reason when available. Tool-call and tool-result IDs are preserved
  exactly. Reasoning values are not rewritten.
- CloseAsync releases local resources. Cancellation stops local cooperative
  work; Crystal provides no session store, reconnection, or provider-specific
  resumption token. These remain external adapter and caller concerns.
- Consumers may opt into RealtimeOutputValidation for per-output contiguous
  content indexes and completion boundaries. It forwards exact events and does
  not reconnect, reorder, or assemble a response.
- RealtimeSessionValidation can reject individually undeclared output
  modalities, turn mode, tools, or reasoning before OpenAsync. Adapters still
  reject unsupported combinations, initial context, and live input shapes.
- The optional typed realtime client wrapper applies caller-owned middleware
  to OpenAsync only. It returns the adapter's exact session and does not
  intercept sends, receives, closure, or connection state.

## Tool protocol compatibility

- Tool definitions are caller-authored names, descriptions, and JSON schemas.
- Tool call identifiers and names are complete values in non-streaming output.
- Model-generated arguments are raw strings because partial or invalid JSON is
  a valid intermediate model outcome.
- Streaming identifier, name, and argument fields are deltas and preserve their
  arrival order.
- Streaming multimodal tool-call content uses stable indexes and complete typed
  content events; it is not a media-byte stream.
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

IEmbeddingClient remains text-only. IMultimodalEmbeddingClient is an independent
optional profile for ordered typed text, image, audio, and video input. Each
embedding input retains its content-block order, and each request retains its
input order. An adapter must return exactly one vector per request input in
that same order; it must reject an unsupported modality, media source shape,
combination, or cardinality rather than silently removing or rewriting blocks.

Consumers may opt into EmbeddingValidation middleware to reject a wrong vector
count at their client boundary; an adapter remains responsible for correct
vector-to-input correspondence and order.
The optional declared-input-shape preflight can reject individual unsupported
modalities and source kinds without accessing media. It cannot establish that a
combination or batch size is supported by the configured model.

Capabilities advertise individual accepted modalities and source shapes, not
every combination. Media sources retain their existing ownership and expiration
semantics. Crystal does not fetch, transcode, or inspect them. PDFs and generic
attachments are outside this profile.
TokenUsage is present only when provider-reported token accounting is
semantically available for the complete request; media billing units are not
silently converted into tokens.
