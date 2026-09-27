using Crystal.Multimodal;
using Crystal.Realtime;

namespace Crystal.Tests;

public sealed class RealtimeContractTests
{
    [Fact]
    public void SessionRequestSnapshotsCallerChoicesInOrder()
    {
        var modalities = new List<ContentModality>
        {
            ContentModality.Audio,
            ContentModality.Text
        };
        var request = new RealtimeSessionRequest(
            modalities,
            RealtimeTurnMode.Explicit);
        modalities.Reverse();

        Assert.Equal(
            new[] { ContentModality.Audio, ContentModality.Text },
            request.OutputModalities);
        Assert.Equal(RealtimeTurnMode.Explicit, request.TurnMode);
        Assert.Empty(request.InitialContext);
        Assert.Throws<ArgumentException>(() =>
            new RealtimeSessionRequest(
                [ContentModality.Text, ContentModality.Text],
                RealtimeTurnMode.Automatic));
    }

    [Fact]
    public void CapabilitiesRequireDistinctShapesAndAUsableTurnMode()
    {
        var text = new MultimodalContentCapability(ContentModality.Text);
        var capabilities = new RealtimeSessionCapabilities(
            [text], [text],
            supportsAutomaticTurnDetection: true,
            supportsExplicitTurnEnd: false);

        Assert.True(capabilities.SupportsAutomaticTurnDetection);
        Assert.Throws<ArgumentException>(() =>
            new RealtimeSessionCapabilities([text], [text], false, false));
        Assert.Throws<ArgumentException>(() =>
            new RealtimeSessionCapabilities([text, text], [text], true, false));
    }

    [Fact]
    public void LiveSegmentsKeepExactContentAndRejectTextMediaOffsets()
    {
        var exactText = new TextContent("caller text");
        var input = new RealtimeInputContent(exactText);
        var output = new RealtimeOutputContentReceived(
            "output-1", 0, exactText);

        Assert.Same(exactText, input.Content);
        Assert.Same(exactText, output.Content);
        Assert.Equal("output-1", output.OutputId);
        var reason = new FinishReason("interrupted");
        var completed = new RealtimeOutputTurnCompleted("output-1", reason);
        Assert.Same(reason, completed.FinishReason);
        Assert.Throws<ArgumentException>(() =>
            new RealtimeInputContent(exactText, TimeSpan.Zero));
        Assert.Throws<ArgumentException>(() =>
            new RealtimeOutputContentReceived(
                "output-1", 0, exactText, TimeSpan.Zero));
    }
}
