using System.Text.Json;

using Crystal.Multimodal.Tools;
using Crystal.Tools;

namespace Crystal.Tests;

public sealed class ToolExecutorCancellationTests
{
    [Theory]
    [InlineData(ToolExecutionMode.Serial, 1)]
    [InlineData(ToolExecutionMode.Concurrent, 2)]
    public async Task TextPolicyCancellationPreventsToolStart(
        ToolExecutionMode mode,
        int maximumConcurrency)
    {
        using var cancellation = new CancellationTokenSource();
        var tool = new RecordingTextTool("known");
        var executor = new ToolExecutor(
            new ToolCatalog([tool]),
            new ToolExecutionOptions(mode, maximumConcurrency),
            (_, _) =>
            {
                cancellation.Cancel();
                return ValueTask.FromResult(ToolInvocationDecision.Execute);
            });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => executor.ExecuteAsync(
                [new ToolCall("call-1", "known", "{}")],
                cancellation.Token));

        Assert.Equal(0, tool.InvocationCount);
    }

    [Theory]
    [InlineData(MultimodalToolExecutionMode.Serial, 1)]
    [InlineData(MultimodalToolExecutionMode.Concurrent, 2)]
    public async Task MultimodalPolicyCancellationPreventsToolStart(
        MultimodalToolExecutionMode mode,
        int maximumConcurrency)
    {
        using var cancellation = new CancellationTokenSource();
        var tool = new RecordingMultimodalTool("known");
        var executor = new MultimodalToolExecutor(
            new MultimodalToolCatalog([tool]),
            new MultimodalToolExecutionOptions(mode, maximumConcurrency),
            (_, _) =>
            {
                cancellation.Cancel();
                return ValueTask.FromResult(MultimodalToolInvocationDecision.Execute);
            });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => executor.ExecuteAsync(
                [new MultimodalToolCall("call-1", "known", "{}")],
                cancellation.Token));

        Assert.Equal(0, tool.InvocationCount);
    }

    [Fact]
    public async Task TextCancellationBetweenSerialCallsPreventsSecondToolStart()
    {
        using var cancellation = new CancellationTokenSource();
        var tool = new RecordingTextTool("known", () => cancellation.Cancel());
        var executor = new ToolExecutor(
            new ToolCatalog([tool]),
            new ToolExecutionOptions(ToolExecutionMode.Serial, 1));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => executor.ExecuteAsync(
                [
                    new ToolCall("call-1", "known", "{}"),
                    new ToolCall("call-2", "known", "{}")
                ],
                cancellation.Token));

        Assert.Equal(1, tool.InvocationCount);
    }

    [Fact]
    public async Task MultimodalCancellationBetweenSerialCallsPreventsSecondToolStart()
    {
        using var cancellation = new CancellationTokenSource();
        var tool = new RecordingMultimodalTool("known", () => cancellation.Cancel());
        var executor = new MultimodalToolExecutor(
            new MultimodalToolCatalog([tool]),
            new MultimodalToolExecutionOptions(MultimodalToolExecutionMode.Serial, 1));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => executor.ExecuteAsync(
                [
                    new MultimodalToolCall("call-1", "known", "{}"),
                    new MultimodalToolCall("call-2", "known", "{}")
                ],
                cancellation.Token));

        Assert.Equal(1, tool.InvocationCount);
    }

    [Fact]
    public async Task CanceledTextToolFailureSkipsExceptionMapping()
    {
        using var cancellation = new CancellationTokenSource();
        var tool = new RecordingTextTool("known", () =>
        {
            cancellation.Cancel();
            throw new InvalidOperationException("Tool detail must stay private.");
        });
        var mapperCalled = false;
        var executor = new ToolExecutor(
            new ToolCatalog([tool]),
            new ToolExecutionOptions(ToolExecutionMode.Serial, 1),
            exceptionMapper: (_, _, _) =>
            {
                mapperCalled = true;
                return ValueTask.FromResult<ToolOutput?>(new ToolOutput("mapped"));
            });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => executor.ExecuteAsync(
                [new ToolCall("call-1", "known", "{}")],
                cancellation.Token));

        Assert.False(mapperCalled);
    }

    [Fact]
    public async Task CanceledMultimodalToolFailureSkipsExceptionMapping()
    {
        using var cancellation = new CancellationTokenSource();
        var tool = new RecordingMultimodalTool("known", () =>
        {
            cancellation.Cancel();
            throw new InvalidOperationException("Tool detail must stay private.");
        });
        var mapperCalled = false;
        var executor = new MultimodalToolExecutor(
            new MultimodalToolCatalog([tool]),
            new MultimodalToolExecutionOptions(MultimodalToolExecutionMode.Serial, 1),
            exceptionMapper: (_, _, _) =>
            {
                mapperCalled = true;
                return ValueTask.FromResult<MultimodalToolOutput?>(
                    new MultimodalToolOutput([]));
            });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => executor.ExecuteAsync(
                [new MultimodalToolCall("call-1", "known", "{}")],
                cancellation.Token));

        Assert.False(mapperCalled);
    }

    private sealed class RecordingTextTool : ITool
    {
        private readonly Action? _onInvoke;

        public RecordingTextTool(string name, Action? onInvoke = null)
        {
            using var schema = JsonDocument.Parse("{}");
            Definition = new ToolDefinition(name, schema.RootElement);
            _onInvoke = onInvoke;
        }

        public ToolDefinition Definition { get; }

        public int InvocationCount { get; private set; }

        public ValueTask<ToolOutput> InvokeAsync(
            ToolCall call,
            CancellationToken cancellationToken = default)
        {
            InvocationCount++;
            _onInvoke?.Invoke();
            return ValueTask.FromResult(new ToolOutput("caller output"));
        }
    }

    private sealed class RecordingMultimodalTool : IMultimodalTool
    {
        private readonly Action? _onInvoke;

        public RecordingMultimodalTool(string name, Action? onInvoke = null)
        {
            using var schema = JsonDocument.Parse("{}");
            Definition = new ToolDefinition(name, schema.RootElement);
            _onInvoke = onInvoke;
        }

        public ToolDefinition Definition { get; }

        public int InvocationCount { get; private set; }

        public ValueTask<MultimodalToolOutput> InvokeAsync(
            MultimodalToolCall call,
            CancellationToken cancellationToken = default)
        {
            InvocationCount++;
            _onInvoke?.Invoke();
            return ValueTask.FromResult(new MultimodalToolOutput([]));
        }
    }
}
