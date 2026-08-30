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
    private int _remainingModelCalls;
    private int _remainingToolCalls;

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
            request.Reasoning);

        yield return new MultimodalHarnessInvocationStartedEvent(
            SessionId,
            request.InvocationId,
            request.AgentName,
            request.ParentInvocationId,
            sequence++,
            effectiveLimits);

        using var operationSource =
            CancellationTokenSource.CreateLinkedTokenSource(
                _sessionCancellationToken,
                cancellationToken);

        MultimodalAgentRunResult? agentResult = null;

        await foreach (var agentEvent in agent.StreamAsync(
                agentRequest,
                operationSource.Token)
            .ConfigureAwait(false))
        {
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
        }

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

            if (depth > Limits.MaximumDepth)
            {
                return MultimodalHarnessReservation.Denied(
                    MultimodalAgentInvocationOutcome.DepthLimitReached);
            }

            var remainingDuration = GetRemainingDuration();

            if (remainingDuration <= TimeSpan.Zero)
            {
                return MultimodalHarnessReservation.Denied(
                    MultimodalAgentInvocationOutcome.DurationLimitReached);
            }

            if (_remainingModelCalls == 0)
            {
                return MultimodalHarnessReservation.Denied(
                    MultimodalAgentInvocationOutcome.ModelCallLimitReached);
            }

            var modelCalls = Math.Min(
                request.Limits.MaximumModelCalls,
                _remainingModelCalls);
            var toolCalls = Math.Min(
                request.Limits.MaximumToolCalls,
                _remainingToolCalls);
            var duration = request.Limits.MaximumDuration <= remainingDuration
                ? request.Limits.MaximumDuration
                : remainingDuration;
            var effectiveLimits = new MultimodalAgentRunLimits(
                modelCalls,
                toolCalls,
                duration);

            _remainingModelCalls -= modelCalls;
            _remainingToolCalls -= toolCalls;

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

    private TimeSpan GetRemainingDuration()
    {
        var elapsed = _timeProvider.GetElapsedTime(_startedTimestamp);
        return Limits.MaximumDuration - elapsed;
    }

    private void ReleaseUnusedReservation(
        MultimodalHarnessReservation reservation,
        MultimodalAgentRunResult result)
    {
        if (result.ModelCallCount > reservation.ReservedModelCalls
            || result.ToolCallCount > reservation.ReservedToolCalls)
        {
            throw new InvalidOperationException(
                "A multimodal Agent exceeded the limits reserved by the Harness.");
        }

        lock (_sync)
        {
            _remainingModelCalls = checked(
                _remainingModelCalls
                + reservation.ReservedModelCalls
                - result.ModelCallCount);
            _remainingToolCalls = checked(
                _remainingToolCalls
                + reservation.ReservedToolCalls
                - result.ToolCallCount);
        }
    }
}
