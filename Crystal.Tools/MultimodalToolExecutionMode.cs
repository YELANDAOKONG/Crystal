namespace Crystal.Multimodal.Tools;

/// <summary>Selects how one multimodal tool-call batch is scheduled.</summary>
public enum MultimodalToolExecutionMode
{
    /// <summary>Executes calls one at a time in input order.</summary>
    Serial,

    /// <summary>Executes calls under an explicit concurrency bound.</summary>
    Concurrent
}
