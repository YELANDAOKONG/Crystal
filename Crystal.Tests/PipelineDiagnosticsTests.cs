using System.Runtime.CompilerServices;

using Crystal.Pipelines;

namespace Crystal.Tests;

public sealed class PipelineDiagnosticsTests
{
    [Fact]
    public async Task AsyncObservationPreservesResponseAndReportsContentFreeStatus()
    {
        var observations = new List<PipelineObservation>();
        var request = new object();
        var pipeline = new AsyncPipeline<object, object>(
            (actualRequest, _) => Task.FromResult(actualRequest),
            [PipelineDiagnostics.ObserveAsync<object, object>("chat", observations.Add)]);

        var response = await pipeline.InvokeAsync(request);

        Assert.Same(request, response);
        Assert.Equal([PipelineStatus.Started, PipelineStatus.Succeeded],
            observations.Select(static observation => observation.Status));
        Assert.All(observations, observation =>
        {
            Assert.Equal("chat", observation.OperationName);
            Assert.Equal(nameof(PipelineObservation), observation.ToString());
            Assert.True(observation.Elapsed >= TimeSpan.Zero);
        });
    }

    [Fact]
    public async Task AsyncObservationReportsFailureWithoutReplacingException()
    {
        var observations = new List<PipelineObservation>();
        var failure = new InvalidOperationException("private detail");
        var pipeline = new AsyncPipeline<string, string>(
            (_, _) => Task.FromException<string>(failure),
            [PipelineDiagnostics.ObserveAsync<string, string>("chat", observations.Add)]);

        var observed = await Assert.ThrowsAsync<InvalidOperationException>(
            () => pipeline.InvokeAsync("secret request"));

        Assert.Same(failure, observed);
        Assert.Equal(PipelineStatus.Failed, observations[^1].Status);
        Assert.DoesNotContain("private detail", observations[^1].ToString());
        Assert.DoesNotContain("secret request", observations[^1].ToString());
    }

    [Fact]
    public async Task AsyncObservationReportsCancellation()
    {
        var observations = new List<PipelineObservation>();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var pipeline = new AsyncPipeline<string, string>(
            (_, token) => Task.FromCanceled<string>(token),
            [PipelineDiagnostics.ObserveAsync<string, string>("chat", observations.Add)]);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => pipeline.InvokeAsync("request", cancellation.Token));

        Assert.Equal(PipelineStatus.Canceled, observations[^1].Status);
    }

    [Fact]
    public async Task StreamObservationStartsOnEnumerationAndReportsEarlyDisposal()
    {
        var observations = new List<PipelineObservation>();
        var first = new object();
        var second = new object();

        async IAsyncEnumerable<object> Source(
            string request,
            [EnumeratorCancellation] CancellationToken token)
        {
            Assert.Equal("request", request);
            yield return first;
            await Task.Yield();
            token.ThrowIfCancellationRequested();
            yield return second;
        }

        var pipeline = new StreamingPipeline<string, object>(
            Source,
            [PipelineDiagnostics.ObserveStreaming<string, object>(
                "chat stream",
                observations.Add)]);
        var stream = pipeline.StreamAsync("request");
        Assert.Empty(observations);

        await foreach (var item in stream)
        {
            Assert.Same(first, item);
            break;
        }

        Assert.Equal([PipelineStatus.Started, PipelineStatus.Abandoned],
            observations.Select(static observation => observation.Status));
    }

    [Fact]
    public async Task StreamObservationReportsNormalCompletionAndPreservesEventOrder()
    {
        var observations = new List<PipelineObservation>();

        async IAsyncEnumerable<int> Source(
            string request,
            [EnumeratorCancellation] CancellationToken token)
        {
            yield return 1;
            await Task.Yield();
            token.ThrowIfCancellationRequested();
            yield return 2;
        }

        var pipeline = new StreamingPipeline<string, int>(
            Source,
            [PipelineDiagnostics.ObserveStreaming<string, int>(
                "chat stream",
                observations.Add)]);
        var events = new List<int>();
        await foreach (var item in pipeline.StreamAsync("request"))
        {
            events.Add(item);
        }

        Assert.Equal([1, 2], events);
        Assert.Equal(PipelineStatus.Succeeded, observations[^1].Status);
    }

    [Fact]
    public async Task StreamObservationReportsFailureWithoutReplacingException()
    {
        var observations = new List<PipelineObservation>();
        var failure = new InvalidOperationException("private detail");

        async IAsyncEnumerable<int> Source(
            string request,
            [EnumeratorCancellation] CancellationToken token)
        {
            yield return 1;
            await Task.Yield();
            throw failure;
        }

        var pipeline = new StreamingPipeline<string, int>(
            Source,
            [PipelineDiagnostics.ObserveStreaming<string, int>(
                "chat stream",
                observations.Add)]);

        var observed = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await foreach (var item in pipeline.StreamAsync("secret request"))
            {
                Assert.Equal(1, item);
            }
        });

        Assert.Same(failure, observed);
        Assert.Equal(PipelineStatus.Failed, observations[^1].Status);
        Assert.DoesNotContain("private detail", observations[^1].ToString());
    }

    [Fact]
    public async Task StreamObservationReportsCallerCancellation()
    {
        var observations = new List<PipelineObservation>();
        using var cancellation = new CancellationTokenSource();

        async IAsyncEnumerable<int> Source(
            string request,
            [EnumeratorCancellation] CancellationToken token)
        {
            yield return 1;
            await Task.Yield();
            token.ThrowIfCancellationRequested();
            yield return 2;
        }

        var pipeline = new StreamingPipeline<string, int>(
            Source,
            [PipelineDiagnostics.ObserveStreaming<string, int>(
                "chat stream",
                observations.Add)]);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var item in pipeline.StreamAsync("request", cancellation.Token))
            {
                Assert.Equal(1, item);
                cancellation.Cancel();
            }
        });

        Assert.Equal(PipelineStatus.Canceled, observations[^1].Status);
    }
}
