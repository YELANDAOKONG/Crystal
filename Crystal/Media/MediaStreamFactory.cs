namespace Crystal.Media;

/// <summary>Opens a fresh readable media stream at its beginning.</summary>
/// <param name="cancellationToken">A token that cancels opening the stream.</param>
/// <returns>A fresh readable stream whose ownership transfers to the caller.</returns>
public delegate ValueTask<Stream> MediaStreamFactory(
    CancellationToken cancellationToken);
