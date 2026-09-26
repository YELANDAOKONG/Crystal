using System.Text.Json;

namespace Crystal;

/// <summary>Requires a schema-conforming final JSON text response.</summary>
public sealed record JsonOutputRequirement
{
    /// <summary>Initializes a hard JSON Schema output requirement.</summary>
    /// <param name="schema">The caller-authored JSON Schema value.</param>
    public JsonOutputRequirement(JsonElement schema)
    {
        if (schema.ValueKind is not (JsonValueKind.Object or JsonValueKind.True
            or JsonValueKind.False))
        {
            throw new ArgumentException(
                "Output schema must be a JSON object or boolean.",
                nameof(schema));
        }

        Schema = schema.Clone();
    }

    /// <summary>Gets a clone of the exact caller-authored schema.</summary>
    public JsonElement Schema => field.Clone();

    /// <inheritdoc />
    public override string ToString() => nameof(JsonOutputRequirement);
}
