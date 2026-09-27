using System.Runtime.InteropServices;

using Crystal.Generation.Images;
using Crystal.Generation.Operations;

namespace Crystal.Tests;

public sealed class GenerationOperationTests
{
    [Fact]
    public void TicketCopiesEncodedStateAndNeverPrintsIt()
    {
        byte[] encoded = [11, 22, 33];
        var ticket = new GenerationOperationTicket("adapter-format", encoded);
        encoded[0] = 99;

        var firstRead = ticket.Data;
        if (MemoryMarshal.TryGetArray(firstRead, out var accessibleCopy))
        {
            accessibleCopy.Array![accessibleCopy.Offset] = 88;
        }

        Assert.Equal([11, 22, 33], ticket.Data.ToArray());
        Assert.Equal(
            new GenerationOperationTicket("adapter-format", new byte[] { 11, 22, 33 }),
            ticket);
        Assert.Equal(nameof(GenerationOperationTicket), ticket.ToString());
        Assert.Throws<ArgumentException>(() =>
            new GenerationOperationTicket("adapter-format", ReadOnlyMemory<byte>.Empty));
    }

    [Fact]
    public void OnlyCompletedSnapshotCarriesAResponse()
    {
        var ticket = new GenerationOperationTicket("adapter-format", new byte[] { 1 });
        var response = new ImageGenerationResponse([]);

        foreach (var status in new[]
        {
            GenerationOperationStatus.Pending,
            GenerationOperationStatus.Running,
            GenerationOperationStatus.Failed,
            GenerationOperationStatus.Canceled
        })
        {
            Assert.Null(new GenerationOperationSnapshot<ImageGenerationResponse>(
                ticket, status).Response);
            Assert.Throws<ArgumentException>(() =>
                new GenerationOperationSnapshot<ImageGenerationResponse>(
                    ticket, status, response));
        }

        var completed = new GenerationOperationSnapshot<ImageGenerationResponse>(
            ticket, GenerationOperationStatus.Completed, response);

        Assert.Same(response, completed.Response);
        Assert.Throws<ArgumentException>(() =>
            new GenerationOperationSnapshot<ImageGenerationResponse>(
                ticket, GenerationOperationStatus.Completed));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GenerationOperationSnapshot<ImageGenerationResponse>(
                ticket, (GenerationOperationStatus)int.MaxValue));
    }
}
