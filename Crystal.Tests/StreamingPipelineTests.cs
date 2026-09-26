using System.Runtime.CompilerServices;

using Crystal.Pipelines;

namespace Crystal.Tests;

public sealed class StreamingPipelineTests
{
    [Fact]
    public async Task EventsRemainOrderedAndCancellationReachesSource()
    {
        using var cancellation = new CancellationTokenSource();
        var transitions = new List<string>();

        async IAsyncEnumerable<int> Source(
            string request,
            [EnumeratorCancellation] CancellationToken token)
        {
            Assert.Equal("request", request);
            Assert.Equal(cancellation.Token, token);
            transitions.Add("source");
            yield return 1;
            await Task.Yield();
            token.ThrowIfCancellationRequested();
            yield return 2;
        }

        StreamingMiddleware<string, int> middleware = next =>
            (request, token) => Observe(next(request, token));

        async IAsyncEnumerable<int> Observe(IAsyncEnumerable<int> events)
        {
            await foreach (var item in events.ConfigureAwait(false))
            {
                transitions.Add($"event {item}");
                yield return item;
            }
        }

        var pipeline = new StreamingPipeline<string, int>(Source, [middleware]);
        var received = new List<int>();
        await foreach (var item in pipeline.StreamAsync("request", cancellation.Token))
        {
            received.Add(item);
        }

        int[] expectedEvents = [1, 2];
        string[] expectedTransitions = ["source", "event 1", "event 2"];
        Assert.Equal(expectedEvents, received);
        Assert.Equal(expectedTransitions, transitions);
    }

    [Fact]
    public async Task CancellationAfterFirstEventStopsSourceEnumeration()
    {
        using var cancellation = new CancellationTokenSource();

        async IAsyncEnumerable<int> Source(
            string request,
            [EnumeratorCancellation] CancellationToken token)
        {
            Assert.Equal("request", request);
            yield return 1;
            await Task.Yield();
            token.ThrowIfCancellationRequested();
            yield return 2;
        }

        var pipeline = new StreamingPipeline<string, int>(Source, []);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var item in pipeline.StreamAsync("request", cancellation.Token))
            {
                Assert.Equal(1, item);
                cancellation.Cancel();
            }
        });
    }
}
