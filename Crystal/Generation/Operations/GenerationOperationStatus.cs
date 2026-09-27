namespace Crystal.Generation.Operations;

/// <summary>Describes the portable state of a remote generation operation.</summary>
public enum GenerationOperationStatus
{
    /// <summary>The operation has been accepted but has not begun execution.</summary>
    Pending,

    /// <summary>The operation is executing.</summary>
    Running,

    /// <summary>The complete generation response is available.</summary>
    Completed,

    /// <summary>The remote operation ended without a generation response.</summary>
    Failed,

    /// <summary>The remote operation was canceled.</summary>
    Canceled
}
