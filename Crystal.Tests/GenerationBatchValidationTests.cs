using Crystal.Decorators;
using Crystal.Generation.Batches;
using Crystal.Generation.Images;
using Crystal.Generation.Operations;
using Crystal.Pipelines;

namespace Crystal.Tests;

public sealed class GenerationBatchValidationTests
{
    [Fact]
    public async Task SubmissionChecksCompletedCountAgainstExactInput()
    {
        var request = new GenerationBatchRequest<ImageGenerationRequest>(
            [new ImageGenerationRequest([]), new ImageGenerationRequest([])]);
        var valid = Completed(2);
        using var cancellation = new CancellationTokenSource();
        var pipeline = new AsyncPipeline<GenerationBatchRequest<ImageGenerationRequest>,
            GenerationOperationSnapshot<GenerationBatchResponse<ImageGenerationResponse>>>(
            (actualRequest, token) =>
            {
                Assert.Same(request, actualRequest);
                Assert.Equal(cancellation.Token, token);
                return Task.FromResult(valid);
            },
            [GenerationBatchValidation.RequireSubmittedCardinality<
                ImageGenerationRequest, ImageGenerationResponse>()]);

        Assert.Same(valid, await pipeline.InvokeAsync(request, cancellation.Token));

        var invalid = new AsyncPipeline<GenerationBatchRequest<ImageGenerationRequest>,
            GenerationOperationSnapshot<GenerationBatchResponse<ImageGenerationResponse>>>(
            (_, _) => Task.FromResult(Completed(1)),
            [GenerationBatchValidation.RequireSubmittedCardinality<
                ImageGenerationRequest, ImageGenerationResponse>()]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            invalid.InvokeAsync(request));
    }

    [Fact]
    public async Task PollUsesCallerRetainedCountAndPassesPendingSnapshot()
    {
        var ticket = Ticket();
        var pending = new GenerationOperationSnapshot<
            GenerationBatchResponse<ImageGenerationResponse>>(
            ticket, GenerationOperationStatus.Running);
        var pipeline = new AsyncPipeline<GenerationOperationTicket,
            GenerationOperationSnapshot<GenerationBatchResponse<ImageGenerationResponse>>>(
            (actualTicket, _) =>
            {
                Assert.Same(ticket, actualTicket);
                return Task.FromResult(pending);
            },
            [GenerationBatchValidation.RequirePolledCardinality<
                ImageGenerationResponse>(2)]);

        Assert.Same(pending, await pipeline.InvokeAsync(ticket));

        var invalid = new AsyncPipeline<GenerationOperationTicket,
            GenerationOperationSnapshot<GenerationBatchResponse<ImageGenerationResponse>>>(
            (_, _) => Task.FromResult(Completed(1)),
            [GenerationBatchValidation.RequirePolledCardinality<
                ImageGenerationResponse>(2)]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            invalid.InvokeAsync(ticket));
    }

    private static GenerationOperationTicket Ticket() =>
        new("test-format", new byte[] { 1 });

    private static GenerationOperationSnapshot<
        GenerationBatchResponse<ImageGenerationResponse>> Completed(int count)
    {
        var items = Enumerable.Range(0, count)
            .Select(_ => new GenerationBatchItemResult<ImageGenerationResponse>(
                GenerationBatchItemStatus.Completed,
                new ImageGenerationResponse([])));
        return new GenerationOperationSnapshot<
            GenerationBatchResponse<ImageGenerationResponse>>(
            Ticket(),
            GenerationOperationStatus.Completed,
            new GenerationBatchResponse<ImageGenerationResponse>(count, items));
    }
}
