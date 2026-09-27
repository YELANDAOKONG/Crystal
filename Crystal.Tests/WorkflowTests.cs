using Crystal;
using Crystal.Agents;
using Crystal.Chat;
using Crystal.Workflows;

namespace Crystal.Tests;

public sealed class WorkflowTests : IChatClient
{
    [Fact]
    public async Task ConcurrentBranchesJoinInGraphOrderAndEmitOrderedEvents()
    {
        var leftStarted = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var rightStarted = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var start = new WorkflowNode<int, int>(
            "start",
            (_, _) => Task.FromResult<IReadOnlyList<int>>([1, 2]));
        var left = new WorkflowNode<int, string>(
            "left",
            async (inputs, token) =>
            {
                leftStarted.SetResult(true);
                await rightStarted.Task.WaitAsync(
                    TimeSpan.FromSeconds(5), token).ConfigureAwait(false);
                return inputs.Select(static value => $"L{value}").ToArray();
            });
        var right = new WorkflowNode<int, string>(
            "right",
            async (inputs, token) =>
            {
                rightStarted.SetResult(true);
                await leftStarted.Task.WaitAsync(
                    TimeSpan.FromSeconds(5), token).ConfigureAwait(false);
                return inputs.Select(static value => $"R{value}").ToArray();
            });
        var join = new WorkflowNode<string, string>(
            "join",
            (inputs, _) => Task.FromResult(inputs));
        var builder = new WorkflowBuilder<int, int>(start);
        builder.AddEdge(start, left);
        builder.AddEdge(start, right);
        builder.AddEdge(left, join);
        builder.AddEdge(right, join);
        var workflow = builder.Build(join);
        var runId = Guid.NewGuid();
        var request = new WorkflowRunRequest<int>(
            runId,
            [0],
            new WorkflowLimits(maximumSupersteps: 3, maximumConcurrency: 2));
        var events = new List<WorkflowEvent>();

        await foreach (var runEvent in workflow.StreamAsync(request))
        {
            events.Add(runEvent);
        }

        var completed = Assert.IsType<WorkflowCompletedEvent<string>>(
            events[^1]);
        Assert.Equal(
            new[] { "L1", "L2", "R1", "R2" },
            completed.Result.Outputs);
        Assert.Equal(WorkflowStopReason.Completed, completed.Result.StopReason);
        Assert.Equal(3, completed.Result.SuperstepCount);
        Assert.Equal(4, completed.Result.NodeCallCount);
        Assert.All(events, item => Assert.Equal(runId, item.RunId));
        Assert.Equal(
            Enumerable.Range(0, events.Count).Select(static value => (long)value),
            events.Select(static item => item.Sequence));
        Assert.Equal(
            new[] { "start", "left", "right", "join" },
            events.OfType<WorkflowNodeCompletedEvent>()
                .Select(static item => item.NodeName));
        Assert.Equal(8, events.OfType<WorkflowMessageRoutedEvent>().Count());
    }

    [Fact]
    public async Task ConditionsRouteExactMessagesAndInputIsSnapshotted()
    {
        var original = new List<int> { 1, 2, 3 };
        var start = new WorkflowNode<int, int>(
            "start",
            (inputs, _) => Task.FromResult(inputs));
        var even = new WorkflowNode<int, string>(
            "even",
            (inputs, _) => Task.FromResult<IReadOnlyList<string>>(
                inputs.Select(static value => $"E{value}").ToArray()));
        var odd = new WorkflowNode<int, string>(
            "odd",
            (inputs, _) => Task.FromResult<IReadOnlyList<string>>(
                inputs.Select(static value => $"O{value}").ToArray()));
        var terminal = new WorkflowNode<string, string>(
            "terminal",
            (inputs, _) => Task.FromResult(inputs));
        var builder = new WorkflowBuilder<int, int>(start);
        builder.AddEdge(start, even,
            (value, _) => ValueTask.FromResult(value % 2 == 0));
        builder.AddEdge(start, odd,
            (value, _) => ValueTask.FromResult(value % 2 != 0));
        builder.AddEdge(even, terminal);
        builder.AddEdge(odd, terminal);
        var workflow = builder.Build(terminal);
        var request = new WorkflowRunRequest<int>(Guid.NewGuid(), original);
        original.Clear();

        var result = await workflow.RunAsync(request);

        Assert.Equal(new[] { "E2", "O1", "O3" }, result.Outputs);
        Assert.Equal(WorkflowStopReason.Completed, result.StopReason);
    }

    [Fact]
    public async Task CycleStopsAtFiniteSuperstepLimitWithExactPartialOutput()
    {
        var loop = new WorkflowNode<int, int>(
            "loop",
            (inputs, _) => Task.FromResult(inputs));
        var terminal = new WorkflowNode<int, int>(
            "terminal",
            (inputs, _) => Task.FromResult(inputs));
        var builder = new WorkflowBuilder<int, int>(loop);
        builder.AddEdge(loop, loop);
        builder.AddEdge(loop, terminal);
        var workflow = builder.Build(terminal);
        var request = new WorkflowRunRequest<int>(
            Guid.NewGuid(),
            [7],
            new WorkflowLimits(maximumSupersteps: 2));

        var result = await workflow.RunAsync(request);

        Assert.Equal(WorkflowStopReason.SuperstepLimitReached,
            result.StopReason);
        Assert.Equal(new[] { 7 }, result.Outputs);
        Assert.Equal(2, result.SuperstepCount);
        Assert.Equal(3, result.NodeCallCount);
    }

    [Fact]
    public void BuildRejectsDisconnectedAndDuplicateNamedNodes()
    {
        var start = new WorkflowNode<int, int>(
            "start",
            (inputs, _) => Task.FromResult(inputs));
        var disconnected = new WorkflowNode<int, int>(
            "unused",
            (inputs, _) => Task.FromResult(inputs));
        var terminal = new WorkflowNode<int, int>(
            "terminal",
            (inputs, _) => Task.FromResult(inputs));
        var builder = new WorkflowBuilder<int, int>(start);
        builder.AddEdge(start, terminal);
        builder.AddEdge(disconnected, terminal);

        Assert.Throws<InvalidOperationException>(() =>
            builder.Build(terminal));

        var sameName = new WorkflowNode<int, int>(
            "start",
            (inputs, _) => Task.FromResult(inputs));
        var otherBuilder = new WorkflowBuilder<int, int>(start);
        otherBuilder.AddEdge(start, sameName);

        Assert.Throws<InvalidOperationException>(() =>
            otherBuilder.Build(sameName));
    }

    [Fact]
    public void AddEdgeRejectsDuplicateSourceTargetAndCondition()
    {
        var start = new WorkflowNode<int, int>(
            "start", (inputs, _) => Task.FromResult(inputs));
        var terminal = new WorkflowNode<int, int>(
            "terminal", (inputs, _) => Task.FromResult(inputs));
        var builder = new WorkflowBuilder<int, int>(start);
        builder.AddEdge(start, terminal);

        Assert.Throws<ArgumentException>(() => builder.AddEdge(start, terminal));

        Func<int, CancellationToken, ValueTask<bool>> condition =
            (value, _) => ValueTask.FromResult(value > 0);
        builder.AddEdge(start, terminal, condition);
        Assert.Throws<ArgumentException>(() =>
            builder.AddEdge(start, terminal, condition));
    }

    [Fact]
    public async Task ObservedCancellationCannotBecomeNormalCompletion()
    {
        using var cancellation = new CancellationTokenSource();
        var start = new WorkflowNode<int, int>(
            "start",
            (inputs, _) =>
            {
                cancellation.Cancel();
                return Task.FromResult(inputs);
            });
        var workflow = new WorkflowBuilder<int, int>(start).Build(start);
        var request = new WorkflowRunRequest<int>(Guid.NewGuid(), [1]);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            workflow.RunAsync(request, cancellation.Token));
    }

    [Fact]
    public async Task ConditionCancellationPreventsLaterConditions()
    {
        using var cancellation = new CancellationTokenSource();
        var laterConditionCalls = 0;
        var start = new WorkflowNode<int, int>(
            "start",
            (inputs, _) => Task.FromResult(inputs));
        var terminal = new WorkflowNode<int, int>(
            "terminal",
            (inputs, _) => Task.FromResult(inputs));
        var builder = new WorkflowBuilder<int, int>(start);
        builder.AddEdge(start, terminal, (_, _) =>
        {
            cancellation.Cancel();
            return ValueTask.FromResult(false);
        });
        builder.AddEdge(start, terminal, (_, _) =>
        {
            laterConditionCalls++;
            return ValueTask.FromResult(true);
        });
        var workflow = builder.Build(terminal);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            workflow.RunAsync(
                new WorkflowRunRequest<int>(Guid.NewGuid(), [1]),
                cancellation.Token));

        Assert.Equal(0, laterConditionCalls);
    }

    [Fact]
    public async Task DisposingEventStreamPreventsLaterNodeWork()
    {
        var laterCalls = 0;
        var start = new WorkflowNode<int, int>(
            "start",
            (inputs, _) => Task.FromResult(inputs));
        var terminal = new WorkflowNode<int, int>(
            "terminal",
            (inputs, _) =>
            {
                laterCalls++;
                return Task.FromResult(inputs);
            });
        var builder = new WorkflowBuilder<int, int>(start);
        builder.AddEdge(start, terminal);
        var workflow = builder.Build(terminal);
        var request = new WorkflowRunRequest<int>(Guid.NewGuid(), [1]);

        await using (var events = workflow.StreamAsync(request)
            .GetAsyncEnumerator())
        {
            Assert.True(await events.MoveNextAsync());
            Assert.IsType<WorkflowNodeCompletedEvent>(events.Current);
        }

        Assert.Equal(0, laterCalls);
    }

    [Fact]
    public async Task AgentCanRunAsCallerOwnedWorkflowNode()
    {
        var agent = new Agent(this, (_, _) => ValueTask.FromResult(0));
        var node = new WorkflowNode<ChatItem, AgentRunResult>(
            "agent",
            async (inputs, token) =>
            [
                await agent.RunAsync(
                    new AgentRunRequest(
                        Guid.NewGuid(),
                        inputs,
                        AgentRunLimits.Unlimited),
                    token).ConfigureAwait(false)
            ]);
        var workflow = new WorkflowBuilder<ChatItem, AgentRunResult>(node)
            .Build(node);
        var original = new ChatMessage(ChatRole.User, "exact input");

        var result = await workflow.RunAsync(
            new WorkflowRunRequest<ChatItem>(Guid.NewGuid(), [original]));

        var agentResult = Assert.Single(result.Outputs);
        Assert.Same(original, agentResult.Transcript[0]);
        Assert.Equal("exact output",
            Assert.IsType<ChatMessage>(agentResult.Transcript[1]).Text);
    }

    public Task<ChatResponse> CompleteAsync(
        ChatRequest request,
        CancellationToken cancellationToken = default)
    {
        Assert.Equal("exact input",
            Assert.IsType<ChatMessage>(Assert.Single(request.Items)).Text);
        return Task.FromResult(new ChatResponse(
            [new ChatCandidate(
                [new ChatMessage(ChatRole.Assistant, "exact output")],
                FinishReason.Stop)]));
    }
}
