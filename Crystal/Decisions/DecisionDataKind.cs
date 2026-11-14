namespace Crystal.Decisions;

/// <summary>Identifies a non-media decision content shape.</summary>
public enum DecisionDataKind
{
    /// <summary>Exact caller-authored text.</summary>
    Text,

    /// <summary>A structured JSON object or array.</summary>
    Json
}
