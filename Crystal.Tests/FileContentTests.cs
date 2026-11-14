using Crystal.Generation;
using Crystal.Media;
using Crystal.Multimodal;
using Crystal.Realtime;

namespace Crystal.Tests;

public sealed class FileContentTests
{
    [Fact]
    public void FileContentPreservesExactSourceAndMetadata()
    {
        var source = new InlineMediaSource(new byte[] { 1 });
        var mimeType = new MediaMimeType("application/pdf");
        var file = new FileMedia(source, mimeType, "Quarterly report.pdf");
        var content = new FileContent(file);

        Assert.Same(file, content.File);
        Assert.Same(ContentModality.File, content.Modality);
        Assert.Same(source, file.Source);
        Assert.Same(mimeType, file.MimeType);
        Assert.Equal("Quarterly report.pdf", file.FileName);
        Assert.Equal(nameof(FileContent), content.ToString());
        Assert.Equal(nameof(FileMedia), file.ToString());
        Assert.Null(new FileMedia(source, mimeType).FileName);
        Assert.Equal("fileName", Assert.Throws<ArgumentException>(
            () => new FileMedia(source, mimeType, " ")).ParamName);
    }

    [Fact]
    public void GenerationRejectsFileCapabilitiesAndOutput()
    {
        var file = new FileContent(new FileMedia(
            new InlineMediaSource(new byte[] { 1 }),
            new MediaMimeType("application/pdf")));
        var capability = new MultimodalContentCapability(
            ContentModality.File, [MediaSourceKind.Inline]);
        var reasoning = new MultimodalReasoningContent(
            [new MultimodalReasoningPart(file, MultimodalReasoningKind.Summary)]);

        Assert.Equal("content", Assert.Throws<ArgumentException>(() =>
            new GenerationInputCapability(capability, GenerationInputPurpose.Source)).ParamName);
        Assert.Equal("outputs", Assert.Throws<ArgumentException>(() =>
            new GenerationCapabilities([], [capability])).ParamName);
        Assert.Equal("content", Assert.Throws<ArgumentException>(() =>
            new GenerationContentItem(file)).ParamName);
        Assert.Equal("content", Assert.Throws<ArgumentException>(() =>
            new GenerationReasoningItem(reasoning)).ParamName);
    }

    [Fact]
    public void RealtimeRejectsFileSegmentCapabilitiesAndEvents()
    {
        var file = new FileContent(new FileMedia(
            new InlineMediaSource(new byte[] { 1 }),
            new MediaMimeType("application/pdf")));
        var capability = new MultimodalContentCapability(
            ContentModality.File, [MediaSourceKind.Inline]);
        var text = new MultimodalContentCapability(ContentModality.Text);
        var reasoning = new MultimodalReasoningContent(
            [new MultimodalReasoningPart(file, MultimodalReasoningKind.Summary)]);

        Assert.Equal("inputs", Assert.Throws<ArgumentException>(() =>
            new RealtimeSessionCapabilities([capability], [text], true, false)).ParamName);
        Assert.Equal("outputs", Assert.Throws<ArgumentException>(() =>
            new RealtimeSessionCapabilities([text], [capability], true, false)).ParamName);
        Assert.Equal("outputModalities", Assert.Throws<ArgumentException>(() =>
            new RealtimeSessionRequest([ContentModality.File], RealtimeTurnMode.Automatic)).ParamName);
        Assert.Equal("content", Assert.Throws<ArgumentException>(() =>
            new RealtimeInputContent(file)).ParamName);
        Assert.Equal("content", Assert.Throws<ArgumentException>(() =>
            new RealtimeOutputContentReceived("output", 0, file)).ParamName);
        Assert.Equal("content", Assert.Throws<ArgumentException>(() =>
            new RealtimeOutputReasoningReceived("output", 0, reasoning)).ParamName);
    }
}
