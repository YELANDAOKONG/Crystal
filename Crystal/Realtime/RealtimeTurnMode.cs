namespace Crystal.Realtime;

/// <summary>Chooses who determines input turn boundaries.</summary>
public enum RealtimeTurnMode
{
    /// <summary>The configured remote service detects input turn boundaries.</summary>
    Automatic,

    /// <summary>The caller sends an explicit input turn-end event.</summary>
    Explicit
}
