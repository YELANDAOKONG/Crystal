using System.Runtime.CompilerServices;

using Crystal.Internal;
using Crystal.Multimodal.Chat;
using Crystal.Multimodal.Tools;

namespace Crystal.Multimodal.Agents;

/// <summary>
/// Runs an explicit, bounded, prompt-free multimodal model and tool loop.
/// Crystal replays media values exactly and does not fetch, transcode, or cache
/// them.
/// </summary>
public sealed class MultimodalAgent : IMultimodalAgent
{
    private readonly MultimodalChatCandidateSelector _candidateSelector;
    private readonly IMultimodalChatClient _client;
    private readonly IMultimodalToolExecutor? _toolExecutor;

    /// <summary>Initializes a multimodal Agent.</summary>
    /// <param name="client">The provider-neutral multimodal Chat client.</param>
    /// <param name="candidateSelector">
    /// The caller-owned candidate-selection policy.
    /// </param>
    /// <param name="toolExecutor">
    /// An optional explicitly configured multimodal tool executor.
    /// </param>
    public MultimodalAgent(
        IMultimodalChatClient client,
        MultimodalChatCandidateSelector candidateSelector,
        IMultimodalToolExecutor? toolExecutor = null)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        ArgumentNullException.ThrowIfNull(
            candidateSelector,
            nameof(candidateSelector));

        Capabilities = client.Capabilities
            ?? throw new ArgumentException(
                "The multimodal Chat client returned no capabilities.",
                nameof(client));
        _client = client;
        _candidateSelector = candidateSelector;
        _toolExecutor = toolExecutor;
    }

    /// <inheritdoc />
    public MultimodalChatCapabilities Capabilities { get; }

    /// <inheritdoc />
    public async Task<MultimodalAgentRunResult> RunAsync(
        MultimodalAgentRunRequest request,
        CancellationToken cancellationToken = default)
    {
        MultimodalAgentRunResult? result = null;

        await foreach (var runEvent in StreamAsync(request, cancellationToken)
            .ConfigureAwait(false))
        {
            if (runEvent is MultimodalAgentRunCompletedEvent completedEvent)
            {
                result = completedEvent.Result;
            }
        }

        return result
            ?? throw new InvalidOperationException(
                "The multimodal Agent stream ended without a completion result.");
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<MultimodalAgentRunEvent> StreamAsync(
        MultimodalAgentRunRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        using var durationSource =
            new CancellationTokenSource(request.Limits.MaximumDuration);
        using var operationSource =
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                durationSource.Token);

        var transcript = new List<MultimodalChatItem>(request.Items);
        var usage = new TokenUsageAccumulator();
        var modelCallCount = 0;
        var toolCallCount = 0;
        long sequence = 0;

        while (true)
        {
            if (durationSource.IsCancellationRequested)
            {
                yield return new MultimodalAgentRunCompletedEvent(
                    request.RunId,
                    sequence,
                    CreateResult(
                        request,
                        transcript,
                        MultimodalAgentRunStopReason.DurationLimitReached,
                        modelCallCount,
                        toolCallCount,
                        usage));
                yield break;
            }

            if (modelCallCount >= request.Limits.MaximumModelCalls)
            {
                yield return new MultimodalAgentRunCompletedEvent(
                    request.RunId,
                    sequence,
                    CreateResult(
                        request,
                        transcript,
                        MultimodalAgentRunStopReason.ModelCallLimitReached,
                        modelCallCount,
                        toolCallCount,
                        usage));
                yield break;
            }

            var chatRequest = new MultimodalChatRequest(
                transcript,
                _toolExecutor?.Definitions,
                request.Reasoning);
            modelCallCount = checked(modelCallCount + 1);

            yield return new MultimodalAgentModelRequestEvent(
                request.RunId,
                sequence++,
                modelCallCount,
                chatRequest);

            var modelOperation = await ExecuteOperationAsync(
                    token => _client.CompleteAsync(chatRequest, token),
                    cancellationToken,
                    durationSource.Token,
                    operationSource.Token)
                .ConfigureAwait(false);

            if (modelOperation.TimedOut)
            {
                usage.Add(null);
                yield return new MultimodalAgentRunCompletedEvent(
                    request.RunId,
                    sequence,
                    CreateResult(
                        request,
                        transcript,
                        MultimodalAgentRunStopReason.DurationLimitReached,
                        modelCallCount,
                        toolCallCount,
                        usage));
                yield break;
            }

            var response = modelOperation.Value
                ?? throw new InvalidOperationException(
                    "The multimodal Chat client returned no response.");
            usage.Add(response.Usage);

            yield return new MultimodalAgentModelResponseEvent(
                request.RunId,
                sequence++,
                modelCallCount,
                response);

            var selectionOperation = await ExecuteOperationAsync(
                    token => _candidateSelector(response, token).AsTask(),
                    cancellationToken,
                    durationSource.Token,
                    operationSource.Token)
                .ConfigureAwait(false);

            if (selectionOperation.TimedOut)
            {
                yield return new MultimodalAgentRunCompletedEvent(
                    request.RunId,
                    sequence,
                    CreateResult(
                        request,
                        transcript,
                        MultimodalAgentRunStopReason.DurationLimitReached,
                        modelCallCount,
                        toolCallCount,
                        usage));
                yield break;
            }

            var selectedCandidateIndex = selectionOperation.Value;

            if (selectedCandidateIndex < 0
                || selectedCandidateIndex >= response.Candidates.Count)
            {
                throw new InvalidOperationException(
                    "The multimodal candidate selector returned an out-of-range index.");
            }

            var candidate = response.Candidates[selectedCandidateIndex];
            transcript.AddRange(candidate.Items);

            yield return new MultimodalAgentCandidateSelectedEvent(
                request.RunId,
                sequence++,
                modelCallCount,
                selectedCandidateIndex,
                candidate);

            var toolCalls = candidate.Items
                .OfType<MultimodalToolCall>()
                .ToArray();

            if (toolCalls.Length == 0)
            {
                yield return new MultimodalAgentRunCompletedEvent(
                    request.RunId,
                    sequence,
                    CreateResult(
                        request,
                        transcript,
                        MultimodalAgentRunStopReason.Completed,
                        modelCallCount,
                        toolCallCount,
                        usage,
                        candidate.FinishReason));
                yield break;
            }

            var remainingToolCalls =
                request.Limits.MaximumToolCalls - toolCallCount;

            if (toolCalls.Length > remainingToolCalls)
            {
                yield return new MultimodalAgentRunCompletedEvent(
                    request.RunId,
                    sequence,
                    CreateResult(
                        request,
                        transcript,
                        MultimodalAgentRunStopReason.ToolCallLimitReached,
                        modelCallCount,
                        toolCallCount,
                        usage));
                yield break;
            }

            if (_toolExecutor is null)
            {
                throw new InvalidOperationException(
                    "The selected multimodal candidate requested tools, "
                    + "but no multimodal tool executor is configured.");
            }

            toolCallCount = checked(toolCallCount + toolCalls.Length);
            var callSnapshot = Array.AsReadOnly(toolCalls);

            yield return new MultimodalAgentToolExecutionStartedEvent(
                request.RunId,
                sequence++,
                modelCallCount,
                callSnapshot);

            var toolOperation = await ExecuteOperationAsync(
                    token => _toolExecutor.ExecuteAsync(callSnapshot, token),
                    cancellationToken,
                    durationSource.Token,
                    operationSource.Token)
                .ConfigureAwait(false);

            if (toolOperation.TimedOut)
            {
                yield return new MultimodalAgentRunCompletedEvent(
                    request.RunId,
                    sequence,
                    CreateResult(
                        request,
                        transcript,
                        MultimodalAgentRunStopReason.DurationLimitReached,
                        modelCallCount,
                        toolCallCount,
                        usage));
                yield break;
            }

            var toolResults = SnapshotAndValidateToolResults(
                callSnapshot,
                toolOperation.Value);
            transcript.AddRange(toolResults);

            yield return new MultimodalAgentToolExecutionCompletedEvent(
                request.RunId,
                sequence++,
                modelCallCount,
                toolResults);
        }
    }

    private static MultimodalAgentRunResult CreateResult(
        MultimodalAgentRunRequest request,
        IEnumerable<MultimodalChatItem> transcript,
        MultimodalAgentRunStopReason stopReason,
        int modelCallCount,
        int toolCallCount,
        TokenUsageAccumulator usage,
        FinishReason? finishReason = null) =>
        new(
            request.RunId,
            transcript,
            stopReason,
            modelCallCount,
            toolCallCount,
            usage.Build(),
            finishReason);

    private static async Task<AgentOperationResult<T>> ExecuteOperationAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken callerToken,
        CancellationToken durationToken,
        CancellationToken operationToken)
    {
        try
        {
            var value = await operation(operationToken).ConfigureAwait(false);
            return AgentOperationResult<T>.Success(value);
        }
        catch (OperationCanceledException)
            when (!callerToken.IsCancellationRequested
                && durationToken.IsCancellationRequested)
        {
            return AgentOperationResult<T>.Timeout();
        }
    }

    private static IReadOnlyList<MultimodalToolResult>
        SnapshotAndValidateToolResults(
            IReadOnlyList<MultimodalToolCall> calls,
            IReadOnlyList<MultimodalToolResult>? results)
    {
        if (results is null)
        {
            throw new InvalidOperationException(
                "The multimodal tool executor returned no result collection.");
        }

        if (results.Count != calls.Count)
        {
            throw new InvalidOperationException(
                "The multimodal tool executor returned an unexpected result count.");
        }

        var snapshot = new MultimodalToolResult[results.Count];

        for (var index = 0; index < results.Count; index++)
        {
            var result = results[index]
                ?? throw new InvalidOperationException(
                    "The multimodal tool executor returned a null result.");

            if (!string.Equals(
                    calls[index].CallId,
                    result.CallId,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "The multimodal tool executor returned results out of correlation order.");
            }

            snapshot[index] = result;
        }

        return Array.AsReadOnly(snapshot);
    }
}
