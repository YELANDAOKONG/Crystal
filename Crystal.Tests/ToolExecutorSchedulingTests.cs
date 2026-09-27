using System.Text.Json;

using Crystal.Multimodal;
using Crystal.Multimodal.Tools;
using Crystal.Tools;

namespace Crystal.Tests;

public sealed class ToolExecutorSchedulingTests
{
    [Fact]
    public async Task TextFailureStopsQueuedCallsWhileStartedCallFinishes()
    {
        var gate = new CallGate();
        var executor = new ToolExecutor(
            new ToolCatalog([new GatedTextTool(gate, "first")]),
            new ToolExecutionOptions(ToolExecutionMode.Concurrent, 2));
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var execution = executor.ExecuteAsync(
            [
                new ToolCall("first", "known", "{}"),
                new ToolCall("second", "known", "{}"),
                new ToolCall("third", "known", "{}")
            ], cancellation.Token);

        await gate.WaitForStartAsync("first").WaitAsync(cancellation.Token);
        await gate.WaitForStartAsync("second").WaitAsync(cancellation.Token);
        gate.Release("first");
        await Task.Delay(50, cancellation.Token);
        gate.Release("second");

        await Assert.ThrowsAsync<ToolInvocationException>(async () =>
            await execution);
        Assert.False(gate.HasStarted("third"));
    }

    [Fact]
    public async Task MultimodalFailureStopsQueuedCallsWhileStartedCallFinishes()
    {
        var gate = new CallGate();
        var executor = new MultimodalToolExecutor(
            new MultimodalToolCatalog([new GatedMultimodalTool(gate, "first")]),
            new MultimodalToolExecutionOptions(
                MultimodalToolExecutionMode.Concurrent, 2));
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var execution = executor.ExecuteAsync(
            [
                new MultimodalToolCall("first", "known", "{}"),
                new MultimodalToolCall("second", "known", "{}"),
                new MultimodalToolCall("third", "known", "{}")
            ], cancellation.Token);

        await gate.WaitForStartAsync("first").WaitAsync(cancellation.Token);
        await gate.WaitForStartAsync("second").WaitAsync(cancellation.Token);
        gate.Release("first");
        await Task.Delay(50, cancellation.Token);
        gate.Release("second");

        await Assert.ThrowsAsync<MultimodalToolInvocationException>(async () =>
            await execution);
        Assert.False(gate.HasStarted("third"));
    }

    [Fact]
    public async Task TextConcurrentExecutionBoundsStartsAndPreservesResultOrder()
    {
        var gate = new CallGate();
        var tool = new GatedTextTool(gate);
        var executor = new ToolExecutor(
            new ToolCatalog([tool]),
            new ToolExecutionOptions(ToolExecutionMode.Concurrent, 2));
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var execution = executor.ExecuteAsync(
            [
                new ToolCall("first", "known", "{}"),
                new ToolCall("second", "known", "{}"),
                new ToolCall("third", "known", "{}")
            ],
            cancellation.Token);

        await gate.WaitForStartAsync("first").WaitAsync(cancellation.Token);
        await gate.WaitForStartAsync("second").WaitAsync(cancellation.Token);
        var thirdStartedBeforeCapacityWasReleased = gate.HasStarted("third");
        gate.Release("second");
        await gate.WaitForStartAsync("third").WaitAsync(cancellation.Token);
        gate.Release("third");
        gate.Release("first");

        var results = await execution;

        Assert.False(thirdStartedBeforeCapacityWasReleased);
        Assert.Equal(
            ["first", "second", "third"],
            results.Select(static result => result.CallId));
        Assert.Equal(
            ["result-first", "result-second", "result-third"],
            results.Select(static result => result.Text));
    }

    [Fact]
    public async Task MultimodalConcurrentExecutionBoundsStartsAndPreservesResultOrder()
    {
        var gate = new CallGate();
        var tool = new GatedMultimodalTool(gate);
        var executor = new MultimodalToolExecutor(
            new MultimodalToolCatalog([tool]),
            new MultimodalToolExecutionOptions(MultimodalToolExecutionMode.Concurrent, 2));
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var execution = executor.ExecuteAsync(
            [
                new MultimodalToolCall("first", "known", "{}"),
                new MultimodalToolCall("second", "known", "{}"),
                new MultimodalToolCall("third", "known", "{}")
            ],
            cancellation.Token);

        await gate.WaitForStartAsync("first").WaitAsync(cancellation.Token);
        await gate.WaitForStartAsync("second").WaitAsync(cancellation.Token);
        var thirdStartedBeforeCapacityWasReleased = gate.HasStarted("third");
        gate.Release("second");
        await gate.WaitForStartAsync("third").WaitAsync(cancellation.Token);
        gate.Release("third");
        gate.Release("first");

        var results = await execution;

        Assert.False(thirdStartedBeforeCapacityWasReleased);
        Assert.Equal(
            ["first", "second", "third"],
            results.Select(static result => result.CallId));
        Assert.Equal(
            ["result-first", "result-second", "result-third"],
            results.Select(static result =>
                Assert.IsType<TextContent>(Assert.Single(result.Contents)).Text));
    }

    private sealed class CallGate
    {
        private readonly Dictionary<string, TaskCompletionSource<bool>> _releases = [];
        private readonly Dictionary<string, TaskCompletionSource<bool>> _starts = [];

        public CallGate()
        {
            foreach (var callId in new[] { "first", "second", "third" })
            {
                _starts.Add(callId, CreateSource());
                _releases.Add(callId, CreateSource());
            }
        }

        public bool HasStarted(string callId) => _starts[callId].Task.IsCompleted;

        public Task WaitForStartAsync(string callId) => _starts[callId].Task;

        public void Release(string callId) => _releases[callId].TrySetResult(true);

        public async Task WaitForReleaseAsync(
            string callId,
            CancellationToken cancellationToken)
        {
            _starts[callId].TrySetResult(true);
            await _releases[callId].Task.WaitAsync(cancellationToken);
        }

        private static TaskCompletionSource<bool> CreateSource() =>
            new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private sealed class GatedTextTool : ITool
    {
        private readonly CallGate _gate;
        private readonly string? _failingCallId;

        public GatedTextTool(CallGate gate, string? failingCallId = null)
        {
            _gate = gate;
            _failingCallId = failingCallId;
            using var schema = JsonDocument.Parse("{}");
            Definition = new ToolDefinition("known", schema.RootElement);
        }

        public ToolDefinition Definition { get; }

        public async ValueTask<ToolOutput> InvokeAsync(
            ToolCall call,
            CancellationToken cancellationToken = default)
        {
            await _gate.WaitForReleaseAsync(call.CallId, cancellationToken);
            if (call.CallId == _failingCallId)
            {
                throw new InvalidOperationException("Tool failed.");
            }

            return new ToolOutput($"result-{call.CallId}");
        }
    }

    private sealed class GatedMultimodalTool : IMultimodalTool
    {
        private readonly CallGate _gate;
        private readonly string? _failingCallId;

        public GatedMultimodalTool(CallGate gate, string? failingCallId = null)
        {
            _gate = gate;
            _failingCallId = failingCallId;
            using var schema = JsonDocument.Parse("{}");
            Definition = new ToolDefinition("known", schema.RootElement);
        }

        public ToolDefinition Definition { get; }

        public async ValueTask<MultimodalToolOutput> InvokeAsync(
            MultimodalToolCall call,
            CancellationToken cancellationToken = default)
        {
            await _gate.WaitForReleaseAsync(call.CallId, cancellationToken);
            if (call.CallId == _failingCallId)
            {
                throw new InvalidOperationException("Tool failed.");
            }

            return new MultimodalToolOutput([new TextContent($"result-{call.CallId}")]);
        }
    }
}
