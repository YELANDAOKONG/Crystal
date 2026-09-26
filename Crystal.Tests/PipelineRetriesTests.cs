using Crystal.Pipelines;

namespace Crystal.Tests;

public sealed class PipelineRetriesTests
{
    [Fact]
    public async Task CallerApprovesEachRetryAndExactRequestIsReplayed()
    {
        var request = new object();
        var response = new object();
        var failure = new InvalidOperationException("Unavailable.");
        var requests = new List<object>();
        var decisions = new List<int>();
        using var cancellation = new CancellationTokenSource();
        AsyncOperation<object, object> terminal = (item, token) =>
        {
            Assert.Equal(cancellation.Token, token);
            requests.Add(item);
            return requests.Count < 3
                ? Task.FromException<object>(failure)
                : Task.FromResult(response);
        };
        var middleware = PipelineRetries.OnException<object, object>(
            3,
            (item, attempt, error, token) =>
            {
                Assert.Same(request, item);
                Assert.Same(failure, error);
                Assert.Equal(cancellation.Token, token);
                decisions.Add(attempt);
                return ValueTask.FromResult(true);
            });
        var pipeline = new AsyncPipeline<object, object>(terminal, [middleware]);

        var actual = await pipeline.InvokeAsync(request, cancellation.Token);

        Assert.Same(response, actual);
        Assert.Equal(new[] { 1, 2 }, decisions);
        Assert.Equal(3, requests.Count);
        Assert.All(requests, item => Assert.Same(request, item));
    }

    [Fact]
    public async Task DeclinedRetryPropagatesOriginalFailure()
    {
        var failure = new InvalidOperationException("Unavailable.");
        var attempts = 0;
        AsyncOperation<string, string> terminal = (_, _) =>
        {
            attempts++;
            return Task.FromException<string>(failure);
        };
        var pipeline = new AsyncPipeline<string, string>(
            terminal,
            [PipelineRetries.OnException<string, string>(
                3,
                (_, _, _, _) => ValueTask.FromResult(false))]);

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            pipeline.InvokeAsync("exact"));

        Assert.Same(failure, actual);
        Assert.Equal(1, attempts);
    }

    [Fact]
    public async Task CancellationIsNeverRetried()
    {
        var decisions = 0;
        var attempts = 0;
        AsyncOperation<string, string> terminal = (_, _) =>
        {
            attempts++;
            return Task.FromException<string>(new OperationCanceledException());
        };
        var pipeline = new AsyncPipeline<string, string>(
            terminal,
            [PipelineRetries.OnException<string, string>(
                3,
                (_, _, _, _) =>
                {
                    decisions++;
                    return ValueTask.FromResult(true);
                })]);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            pipeline.InvokeAsync("exact"));

        Assert.Equal(1, attempts);
        Assert.Equal(0, decisions);
    }

    [Fact]
    public async Task ObservedCancellationCannotBecomeSuccessOrAnotherAttempt()
    {
        using var cancellation = new CancellationTokenSource();
        var attempts = 0;
        var decisions = 0;
        AsyncOperation<string, string> terminal = (_, _) =>
        {
            attempts++;
            cancellation.Cancel();
            return Task.FromResult("late");
        };
        var pipeline = new AsyncPipeline<string, string>(
            terminal,
            [PipelineRetries.OnException<string, string>(
                3,
                (_, _, _, _) =>
                {
                    decisions++;
                    return ValueTask.FromResult(true);
                })]);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            pipeline.InvokeAsync("exact", cancellation.Token));

        Assert.Equal(1, attempts);
        Assert.Equal(0, decisions);
    }

    [Fact]
    public async Task MaximumAttemptsStopsFurtherCallsWithoutDecision()
    {
        var attempts = 0;
        var decisions = 0;
        AsyncOperation<string, string> terminal = (_, _) =>
        {
            attempts++;
            return Task.FromException<string>(
                new InvalidOperationException("Unavailable."));
        };
        var pipeline = new AsyncPipeline<string, string>(
            terminal,
            [PipelineRetries.OnException<string, string>(
                2,
                (_, _, _, _) =>
                {
                    decisions++;
                    return ValueTask.FromResult(true);
                })]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            pipeline.InvokeAsync("exact"));

        Assert.Equal(2, attempts);
        Assert.Equal(1, decisions);
    }
}
