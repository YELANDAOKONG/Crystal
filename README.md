# Crystal

Crystal is a provider-neutral C# library for text completion, chat, embeddings,
tool execution, bounded Agents, and explicit Agent Harness composition.

The current development line is text-only. Multimodal Chat, multimodal Agents,
and image, audio, and video generation are committed future capabilities. They
will be supported after the text foundation, as additive typed contracts.

## Design guarantees

- No built-in provider, authentication, transport, or model catalog.
- No built-in prompt or runtime-authored model text.
- No concrete tools.
- Ordered readable and opaque reasoning preservation.
- Explicit candidate, tool, approval, limit, and composition policies.
- Immutable public data contracts.

## Project status

Crystal targets net10.0 and has no compatibility baseline yet. The current
repository implements the text foundation described in ROADMAP.md. A test project
has not yet been authorized; the executable quality gate is:

~~~bash
dotnet build Crystal.sln
~~~

## Using Crystal

Crystal does not connect to a model provider by itself. An external adapter
implements one or more of IChatClient, ICompletionClient, IEmbeddingClient, and
their optional streaming interfaces. Provider selection, model identifiers,
credentials, temperature, Top-P, and other wire options stay in that adapter.

During local development, a consumer can reference the project directly:

~~~xml
<ItemGroup>
  <ProjectReference Include="../Crystal/Crystal.csproj" />
</ItemGroup>
~~~

### Chat

A system prompt is an ordinary caller-authored system message. Crystal preserves
it exactly and never adds another prompt:

~~~csharp
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Crystal.Chat;

namespace Example;

public static class ChatExample
{
    public static async Task<string> AskAsync(
        IChatClient client,
        string systemPrompt,
        string question,
        CancellationToken cancellationToken)
    {
        var request = new ChatRequest(
        [
            new ChatMessage(ChatRole.System, systemPrompt),
            new ChatMessage(ChatRole.User, question)
        ]);

        var response = await client.CompleteAsync(
            request,
            cancellationToken);
        var candidate = response.Candidates[0];

        return string.Concat(
            candidate.Items
                .OfType<ChatMessage>()
                .Where(static message =>
                    message.Role == ChatRole.Assistant)
                .Select(static message => message.Text));
    }
}
~~~

The example explicitly selects candidate zero. Applications that request
multiple candidates must own their selection policy.

### Completion and embeddings

~~~csharp
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Crystal.Completions;
using Crystal.Embeddings;

namespace Example;

public static class DirectModelExample
{
    public static async Task<string> CompleteAsync(
        ICompletionClient client,
        string prompt,
        CancellationToken cancellationToken)
    {
        var response = await client.CompleteAsync(
            new CompletionRequest(prompt),
            cancellationToken);

        return string.Concat(
            response.Candidates[0].Items
                .OfType<CompletionText>()
                .Select(static item => item.Text));
    }

    public static async Task<ReadOnlyMemory<float>> EmbedAsync(
        IEmbeddingClient client,
        string text,
        CancellationToken cancellationToken)
    {
        var response = await client.EmbedAsync(
            new EmbeddingRequest([text]),
            cancellationToken);

        return response.Vectors[0].Values;
    }
}
~~~

### Tools and Agent

Callers implement ITool, register tools in an immutable catalog, choose serial or
bounded-concurrent execution, and provide a candidate-selection policy:

~~~csharp
using System;
using System.Threading;
using System.Threading.Tasks;

using Crystal.Agents;
using Crystal.Chat;
using Crystal.Tools;

namespace Example;

public static class AgentExample
{
    private const int MaximumModelCalls = 8;
    private const int MaximumToolCalls = 8;

    public static Task<AgentRunResult> RunAsync(
        IChatClient client,
        ITool tool,
        string systemPrompt,
        string requestText,
        CancellationToken cancellationToken)
    {
        var executor = new ToolExecutor(
            new ToolCatalog([tool]),
            new ToolExecutionOptions(
                ToolExecutionMode.Serial,
                maximumConcurrency: 1));
        IAgent agent = new Agent(
            client,
            static (_, _) => ValueTask.FromResult(0),
            executor);

        return agent.RunAsync(
            new AgentRunRequest(
                Guid.NewGuid(),
                [
                    new ChatMessage(ChatRole.System, systemPrompt),
                    new ChatMessage(ChatRole.User, requestText)
                ],
                new AgentRunLimits(
                    MaximumModelCalls,
                    MaximumToolCalls,
                    TimeSpan.FromMinutes(1))),
            cancellationToken);
    }
}
~~~

ITool receives exact raw model arguments in ToolCall.Arguments and returns exact
caller-owned text in ToolOutput. The Agent never repairs either value. Run usage
is available only when every attempted model call reports usage.

### Harness

Register Agents under case-sensitive names, create a bounded session, and invoke
each Agent explicitly. Parent invocation identifiers express ancestry; Harness
does not choose routes:

~~~csharp
using System;
using System.Threading;
using System.Threading.Tasks;

using Crystal.Agents;
using Crystal.Chat;
using Crystal.Harness;

namespace Example;

public static class HarnessExample
{
    private const int MaximumDepth = 2;
    private const int MaximumModelCalls = 20;
    private const int MaximumToolCalls = 20;
    private static readonly TimeSpan MaximumDuration =
        TimeSpan.FromMinutes(2);

    public static Task<AgentInvocationResult> InvokeAsync(
        IAgent agent,
        AgentRunLimits perAgentLimits,
        string requestText,
        CancellationToken cancellationToken)
    {
        var name = new AgentName("assistant");
        var harness = new AgentHarness(
        [
            new AgentRegistration(name, agent)
        ]);
        var session = harness.CreateSession(
            Guid.NewGuid(),
            new HarnessLimits(
                MaximumDepth,
                MaximumModelCalls,
                MaximumToolCalls,
                MaximumDuration),
            cancellationToken);

        return session.InvokeAsync(
            new AgentInvocationRequest(
                Guid.NewGuid(),
                name,
                [new ChatMessage(ChatRole.User, requestText)],
                perAgentLimits),
            cancellationToken);
    }
}
~~~

To invoke a child, pass the completed or registered parent invocation identifier
through AgentInvocationRequest.ParentInvocationId.

Streaming adapters use candidate and item indexes to preserve interleaving.
Reasoning text deltas also use TextSegmentIndex so multiple readable segments can
be reconstructed without treating transport chunks as semantic boundaries.

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

Crystal will support multimodal Chat and Agents, and image, audio, and video
generation. That work is deferred until the text foundation is usable, and it
will arrive as explicit typed capabilities rather than placeholders in the
current assemblies. Existing text APIs will remain text-only. The project will
not use a generic attachment bag as a temporary compatibility shortcut.
