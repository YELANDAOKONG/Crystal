namespace Crystal.Generation.Batches;

/// <summary>Describes one terminal item state in a completed batch.</summary>
public enum GenerationBatchItemStatus
{
    /// <summary>The item has a complete target-specific response.</summary>
    Completed,

    /// <summary>The item ended without a response.</summary>
    Failed,

    /// <summary>The item was canceled remotely.</summary>
    Canceled
}
