using System.Runtime.CompilerServices;

using Crystal.Multimodal.Agents;

namespace Crystal.Multimodal.Harness;

/// <summary>
/// Enforces shared limits and ancestry for multimodal Agent invocations.
/// </summary>
public sealed class MultimodalAgentHarnessSession
    : IMultimodalAgentHarnessSession
{
    private readonly IReadOnlyDictionary<string, IMultimodalAgent> _agents;
    private readonly Dictionary<Guid, int> _invocationDepths = [];
    private readonly CancellationToken _sessionCancellationToken;
    private readonly long _startedTimestamp;
    private readonly object _sync = new();
    private readonly TimeProvider _timeProvider;
    private int? _remainingModelCalls;
    private int? _remainingToolCalls;

    internal MultimodalAgentHarnessSession(
        Guid sessionId,
        MultimodalHarnessLimits limits,
        IReadOnlyDictionary<string, IMultimodalAgent> agents,
        TimeProvider timeProvider,
        CancellationToken sessionCancellationToken)
    {
        SessionId = sessionId;
        Limits = limits;
        _agents = agents;
        _timeProvider = timeProvider;
        _sessionCancellationToken = sessionCancellationToken;
        _remainingModelCalls = limits.MaximumModelCalls;
        _remainingToolCalls = limits.MaximumToolCalls;
        _startedTimestamp = timeProvider.GetTimestamp();
    }

    /// <inheritdoc />
    public Guid SessionId { get; }

    /// <inheritdoc />
    public MultimodalHarnessLimits Limits { get; }

    /// <inheritdoc />
    public async Task<MultimodalAgentInvocationResult> InvokeAsync(
        MultimodalAgentInvocationRequest request,
        CancellationToken cancellationToken = default)
    {
        MultimodalAgentInvocationResult? result = null;

        await foreach (var harnessEvent in StreamAsync(
                request,
                cancellationToken)
            .ConfigureAwait(false))
        {
            if (harnessEvent
                is MultimodalHarnessInvocationCompletedEvent completedEvent)
            {
                result = completedEvent.Result;
            }
        }

        return result
            ?? throw new InvalidOperationException(
                "The multimodal Harness stream ended without an invocation result.");
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<MultimodalHarnessEvent> StreamAsync(
        MultimodalAgentInvocationRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        cancellationToken.ThrowIfCancellationRequested();
        _sessionCancellationToken.ThrowIfCancellationRequested();

        var reservation = Reserve(request);
        long sequence = 0;

        if (!reservation.IsGranted)
        {
            var deniedResult = new MultimodalAgentInvocationResult(
                SessionId,
                request.InvocationId,
                request.AgentName,
                reservation.DeniedOutcome
                    ?? throw new InvalidOperationException(
                        "The multimodal Harness produced no denial outcome."),
                request.ParentInvocationId);

            yield return new MultimodalHarnessInvocationCompletedEvent(
                SessionId,
                request.InvocationId,
                request.AgentName,
                request.ParentInvocationId,
                sequence,
                deniedResult);
            yield break;
        }

        var agent = reservation.Agent
            ?? throw new InvalidOperationException(
                "The multimodal Harness reserved no Agent.");
        var effectiveLimits = reservation.EffectiveLimits
            ?? throw new InvalidOperationException(
                "The multimodal Harness reserved no effective limits.");
        var agentRequest = new MultimodalAgentRunRequest(
            request.InvocationId,
            request.Items,
            effectiveLimits,
            request.Reasoning,
            request.JsonOutput);

        using var operationSource =
            CancellationTokenSource.CreateLinkedTokenSource(
                _sessionCancellationToken,
                cancellationToken);
        operationSource.Token.ThrowIfCancellationRequested();

        yield return new MultimodalHarnessInvocationStartedEvent(
            SessionId,
            request.InvocationId,
            request.AgentName,
            request.ParentInvocationId,
            sequence++,
            effectiveLimits);
        operationSource.Token.ThrowIfCancellationRequested();

        MultimodalAgentRunResult? agentResult = null;

        await foreach (var agentEvent in agent.StreamAsync(
                agentRequest,
                operationSource.Token)
            .ConfigureAwait(false))
        {
            operationSource.Token.ThrowIfCancellationRequested();
            if (agentResult is not null)
            {
                throw new InvalidOperationException(
                    "A multimodal Agent emitted events after completion.");
            }

            if (agentEvent is MultimodalAgentRunCompletedEvent completedEvent)
            {
                agentResult = completedEvent.Result;
            }

            yield return new MultimodalHarnessAgentEvent(
                SessionId,
                request.InvocationId,
                request.AgentName,
                request.ParentInvocationId,
                sequence++,
                agentEvent);
            operationSource.Token.ThrowIfCancellationRequested();
        }

        operationSource.Token.ThrowIfCancellationRequested();

        if (agentResult is null)
        {
            throw new InvalidOperationException(
                "The multimodal Agent stream ended without a completion result.");
        }

        ReleaseUnusedReservation(reservation, agentResult);

        var completedResult = new MultimodalAgentInvocationResult(
            SessionId,
            request.InvocationId,
            request.AgentName,
            MultimodalAgentInvocationOutcome.Completed,
            request.ParentInvocationId,
            agentResult);

        operationSource.Token.ThrowIfCancellationRequested();

        yield return new MultimodalHarnessInvocationCompletedEvent(
            SessionId,
            request.InvocationId,
            request.AgentName,
            request.ParentInvocationId,
            sequence,
            completedResult);
    }

    private MultimodalHarnessReservation Reserve(
        MultimodalAgentInvocationRequest request)
    {
        lock (_sync)
        {
            if (_invocationDepths.ContainsKey(request.InvocationId))
            {
                throw new ArgumentException(
                    "Invocation identifier already exists in this session.",
                    nameof(request));
            }

            if (!_agents.TryGetValue(request.AgentName.Value, out var agent))
            {
                throw new KeyNotFoundException(
                    "The requested multimodal Agent is not registered.");
            }

            var depth = GetInvocationDepth(request);
            _invocationDepths.Add(request.InvocationId, depth);

            if (Limits.MaximumDepth is int maximumDepth && depth > maximumDepth)
            {
                return MultimodalHarnessReservation.Denied(
                    MultimodalAgentInvocationOutcome.DepthLimitReached);
            }

            var remainingDuration = GetRemainingDuration();

            if (remainingDuration is TimeSpan availableDuration
                && availableDuration <= TimeSpan.Zero)
            {
                return MultimodalHarnessReservation.Denied(
                    MultimodalAgentInvocationOutcome.DurationLimitReached);
            }

            if (_remainingModelCalls == 0)
            {
                return MultimodalHarnessReservation.Denied(
                    MultimodalAgentInvocationOutcome.ModelCallLimitReached);
            }

            var modelCalls = MinLimit(
                request.Limits.MaximumModelCalls,
                _remainingModelCalls);
            var toolCalls = MinLimit(
                request.Limits.MaximumToolCalls,
                _remainingToolCalls);
            var duration = MinLimit(
                request.Limits.MaximumDuration,
                remainingDuration);
            var effectiveLimits = new MultimodalAgentRunLimits(
                modelCalls,
                toolCalls,
                duration);

            if (_remainingModelCalls is int remainingModelCalls)
            {
                _remainingModelCalls = remainingModelCalls
                    - (modelCalls ?? throw new InvalidOperationException(
                        "The multimodal Harness reserved no model-call capacity."));
            }

            if (_remainingToolCalls is int remainingToolCalls)
            {
                _remainingToolCalls = remainingToolCalls
                    - (toolCalls ?? throw new InvalidOperationException(
                        "The multimodal Harness reserved no tool-call capacity."));
            }

            return MultimodalHarnessReservation.Granted(agent, effectiveLimits);
        }
    }

    private int GetInvocationDepth(
        MultimodalAgentInvocationRequest request)
    {
        if (request.ParentInvocationId is not Guid parentInvocationId)
        {
            return 0;
        }

        if (!_invocationDepths.TryGetValue(
                parentInvocationId,
                out var parentDepth))
        {
            throw new ArgumentException(
                "Parent invocation is not registered in this session.",
                nameof(request));
        }

        return checked(parentDepth + 1);
    }

    private TimeSpan? GetRemainingDuration()
    {
        if (Limits.MaximumDuration is not TimeSpan maximumDuration)
        {
            return null;
        }

        var elapsed = _timeProvider.GetElapsedTime(_startedTimestamp);
        return maximumDuration - elapsed;
    }

    private static int? MinLimit(int? first, int? second)
    {
        if (first is null)
        {
            return second;
        }

        return second is null ? first : Math.Min(first.Value, second.Value);
    }

    private static TimeSpan? MinLimit(TimeSpan? first, TimeSpan? second)
    {
        if (first is null)
        {
            return second;
        }

        return second is null ? first : TimeSpan.FromTicks(
            Math.Min(first.Value.Ticks, second.Value.Ticks));
    }

    private void ReleaseUnusedReservation(
        MultimodalHarnessReservation reservation,
        MultimodalAgentRunResult result)
    {
        if ((reservation.ReservedModelCalls is int maximumReservedModelCalls
                && result.ModelCallCount > maximumReservedModelCalls)
            || (reservation.ReservedToolCalls is int maximumReservedToolCalls
                && result.ToolCallCount > maximumReservedToolCalls))
        {
            throw new InvalidOperationException(
                "A multimodal Agent exceeded the limits reserved by the Harness.");
        }

        lock (_sync)
        {
            if (_remainingModelCalls is int remainingModelCalls)
            {
                var reservedModelCalls = reservation.ReservedModelCalls
                    ?? throw new InvalidOperationException(
                        "The multimodal Harness has no model-call reservation to release.");
                _remainingModelCalls = checked(
                    remainingModelCalls
                    + reservedModelCalls
                    - result.ModelCallCount);
            }

            if (_remainingToolCalls is int remainingToolCalls)
            {
                var reservedToolCalls = reservation.ReservedToolCalls
                    ?? throw new InvalidOperationException(
                        "The multimodal Harness has no tool-call reservation to release.");
                _remainingToolCalls = checked(
                    remainingToolCalls
                    + reservedToolCalls
                    - result.ToolCallCount);
            }
        }
    }
}
