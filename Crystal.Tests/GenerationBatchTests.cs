using Crystal.Generation.Batches;
using Crystal.Generation.Images;

namespace Crystal.Tests;

public sealed class GenerationBatchTests
{
    [Fact]
    public void BatchPreservesOriginalRequestAndMixedResultOrder()
    {
        var first = new ImageGenerationRequest([]);
        var second = new ImageGenerationRequest([]);
        var third = new ImageGenerationRequest([]);
        var source = new List<ImageGenerationRequest> { first, second, third };
        var batch = new GenerationBatchRequest<ImageGenerationRequest>(source);
        source.Reverse();

        var firstResponse = new ImageGenerationResponse([]);
        var thirdResponse = new ImageGenerationResponse([]);
        var response = new GenerationBatchResponse<ImageGenerationResponse>(
            batch.Requests.Count,
            [
                new(GenerationBatchItemStatus.Completed, firstResponse),
                new(GenerationBatchItemStatus.Failed),
                new(GenerationBatchItemStatus.Completed, thirdResponse)
            ]);

        Assert.Equal(new[] { first, second, third }, batch.Requests);
        Assert.Same(firstResponse, response.Items[0].Response);
        Assert.Equal(GenerationBatchItemStatus.Failed, response.Items[1].Status);
        Assert.Null(response.Items[1].Response);
        Assert.Same(thirdResponse, response.Items[2].Response);
    }

    [Fact]
    public void BatchRejectsMissingAndContradictoryItemResults()
    {
        Assert.Throws<ArgumentException>(() =>
            new GenerationBatchRequest<ImageGenerationRequest>([]));
        Assert.Throws<ArgumentException>(() =>
            new GenerationBatchResponse<ImageGenerationResponse>(
                2,
                [new GenerationBatchItemResult<ImageGenerationResponse>(
                    GenerationBatchItemStatus.Failed)]));
        Assert.Throws<ArgumentException>(() =>
            new GenerationBatchItemResult<ImageGenerationResponse>(
                GenerationBatchItemStatus.Completed));
        Assert.Throws<ArgumentException>(() =>
            new GenerationBatchItemResult<ImageGenerationResponse>(
                GenerationBatchItemStatus.Canceled,
                new ImageGenerationResponse([])));
    }
}
