using Crystal.Decorators;
using Crystal.Multimodal;
using Crystal.Multimodal.Tools;
using Crystal.Realtime;

namespace Crystal.Tests;

public sealed class RealtimeOutputValidationTests
{
    [Fact]
    public async Task InterleavedOutputsForwardExactEventsInArrivalOrder()
    {
        var first = Content("first", 0, "exact text");
        var second = Content("second", 0, "other text");
        var tool = new RealtimeOutputToolCallReceived(
            new MultimodalToolCall("call-1", "caller-tool", "{}"));
        var later = Content("first", 1, "later text");
        var firstCompleted = new RealtimeOutputTurnCompleted("first");
        var secondCompleted = new RealtimeOutputTurnCompleted("second");
        var expected = new RealtimeOutputEvent[]
        {
            first, second, tool, later, firstCompleted, secondCompleted
        };

        var actual = await CollectAsync(
            RealtimeOutputValidation.ValidateAsync(Events(expected)));

        Assert.Equal(expected.Length, actual.Count);
        for (var index = 0; index < expected.Length; index++)
        {
            Assert.Same(expected[index], actual[index]);
        }
    }

    [Fact]
    public async Task InvalidIndexesAndClosedOutputsAreRejected()
    {
        var invalidStreams = new RealtimeOutputEvent[][]
        {
            [Content("first", 1, "secret")],
            [Content("first", 0, "secret"), Content("first", 0, "secret")],
            [new RealtimeOutputTurnCompleted("first"),
                Content("first", 0, "secret")],
            [new RealtimeOutputTurnCompleted("first"),
                new RealtimeOutputTurnCompleted("first")]
        };

        foreach (var events in invalidStreams)
        {
            var failure = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                CollectAsync(RealtimeOutputValidation.ValidateAsync(Events(events))));

            Assert.DoesNotContain("secret", failure.Message);
        }
    }

    [Fact]
    public async Task EarlyDisposalDoesNotInspectLaterOutput()
    {
        var first = Content("first", 0, "exact text");
        await using (var enumerator = RealtimeOutputValidation.ValidateAsync(
            Events([first, Content("first", 0, "invalid")]))
            .GetAsyncEnumerator())
        {
            Assert.True(await enumerator.MoveNextAsync());
            Assert.Same(first, enumerator.Current);
        }
    }

    [Fact]
    public async Task CancellationCannotForwardLateOutput()
    {
        using var cancellation = new CancellationTokenSource();
        var source = CancelThenYield(cancellation);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CollectAsync(RealtimeOutputValidation.ValidateAsync(
                source, cancellation.Token)));
    }

    private static RealtimeOutputContentReceived Content(
        string outputId,
        int segmentIndex,
        string text) =>
        new(outputId, segmentIndex, new TextContent(text));

    private static async IAsyncEnumerable<RealtimeOutputEvent> Events(
        IEnumerable<RealtimeOutputEvent> events)
    {
        await Task.Yield();
        foreach (var outputEvent in events)
        {
            yield return outputEvent;
        }
    }

    private static async IAsyncEnumerable<RealtimeOutputEvent> CancelThenYield(
        CancellationTokenSource cancellation)
    {
        await Task.Yield();
        cancellation.Cancel();
        yield return Content("first", 0, "late output");
    }

    private static async Task<List<RealtimeOutputEvent>> CollectAsync(
        IAsyncEnumerable<RealtimeOutputEvent> events)
    {
        var collected = new List<RealtimeOutputEvent>();
        await foreach (var outputEvent in events)
        {
            collected.Add(outputEvent);
        }

        return collected;
    }
}
