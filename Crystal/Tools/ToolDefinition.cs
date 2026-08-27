using System.Text.Json;

namespace Crystal.Tools;

/// <summary>
/// Describes one caller-authored tool exposed to a model.
/// </summary>
public sealed record ToolDefinition
{
    /// <summary>
    /// Initializes a tool definition.
    /// </summary>
    /// <param name="name">The stable case-sensitive tool name.</param>
    /// <param name="inputSchema">The caller-authored JSON input schema.</param>
    /// <param name="description">
    /// Optional caller-authored descriptive text.
    /// </param>
    public ToolDefinition(
        string name,
        JsonElement inputSchema,
        string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        if (inputSchema.ValueKind == JsonValueKind.Undefined)
        {
            throw new ArgumentException(
                "Input schema must contain a JSON value.",
                nameof(inputSchema));
        }

        Name = name;
        Description = description;
        InputSchema = inputSchema.Clone();
    }

    /// <summary>
    /// Gets the stable case-sensitive name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the optional caller-authored description.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets a clone of the caller-authored JSON input schema.
    /// </summary>
    public JsonElement InputSchema => field.Clone();

    /// <inheritdoc />
    public override string ToString() => nameof(ToolDefinition);
}
