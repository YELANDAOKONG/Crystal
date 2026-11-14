using System.Text.Json;

namespace Crystal.Decisions;

/// <summary>Contains one exact native content value or a structured JSON value.</summary>
/// <typeparam name="TContent">The content type of the decision profile.</typeparam>
public sealed record DecisionContent<TContent>
    where TContent : class
{
    /// <summary>Initializes a block with exact profile content.</summary>
    /// <param name="content">The caller-supplied content.</param>
    public DecisionContent(TContent content)
    {
        ArgumentNullException.ThrowIfNull(content);
        Content = content;
    }

    /// <summary>Initializes a block with a structured JSON object or array.</summary>
    /// <param name="json">The exact JSON value to snapshot.</param>
    public DecisionContent(JsonElement json)
    {
        if (json.ValueKind is not (JsonValueKind.Object or JsonValueKind.Array))
        {
            throw new ArgumentException("Decision JSON must be an object or array.", nameof(json));
        }

        Json = json.Clone();
    }

    /// <summary>Gets the exact profile content when this is a native content block.</summary>
    public TContent? Content { get; }

    /// <summary>Gets a clone of the exact JSON value when this is a structured block.</summary>
    public JsonElement? Json => field?.Clone();

    /// <inheritdoc />
    public override string ToString() => nameof(DecisionContent<TContent>);
}
