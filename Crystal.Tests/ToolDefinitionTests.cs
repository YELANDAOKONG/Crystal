using System.Text.Json;

using Crystal.Tools;

namespace Crystal.Tests;

public sealed class ToolDefinitionTests
{
    [Fact]
    public void InputSchemaSnapshotsObjectAfterDocumentDisposal()
    {
        ToolDefinition definition;
        using (var document = JsonDocument.Parse(
            "{\"type\":\"object\",\"description\":\"private\"}"))
        {
            definition = new ToolDefinition(
                "caller-tool",
                document.RootElement);
        }

        Assert.Equal("object", definition.InputSchema
            .GetProperty("type").GetString());
        Assert.Equal(nameof(ToolDefinition), definition.ToString());
    }

    [Fact]
    public void InputSchemaAcceptsBooleanAndRejectsOtherRootValues()
    {
        using var booleanSchema = JsonDocument.Parse("true");
        using var invalidSchema = JsonDocument.Parse("[1]");

        Assert.Equal(
            JsonValueKind.True,
            new ToolDefinition("caller-tool", booleanSchema.RootElement)
                .InputSchema.ValueKind);
        Assert.Equal(
            "inputSchema",
            Assert.Throws<ArgumentException>(() =>
                new ToolDefinition("caller-tool", invalidSchema.RootElement))
                .ParamName);
        Assert.Equal(
            "inputSchema",
            Assert.Throws<ArgumentException>(() =>
                new ToolDefinition("caller-tool", default)).ParamName);
    }
}
