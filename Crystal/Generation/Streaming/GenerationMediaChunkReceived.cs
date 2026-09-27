using Crystal.Media;

namespace Crystal.Generation.Streaming;

/// <summary>
/// Reports one encoded byte chunk of a provisional media revision.
/// </summary>
/// <typeparam name="TResponse">The target-specific complete response type.</typeparam>
public sealed record GenerationMediaChunkReceived<TResponse> :
    GenerationStreamEvent<TResponse>
    where TResponse : class
{
    private readonly byte[] _data;

    /// <summary>Initializes one encoded provisional media chunk.</summary>
    /// <param name="candidateIndex">The zero-based candidate index.</param>
    /// <param name="itemIndex">The zero-based item index.</param>
    /// <param name="revisionIndex">The zero-based revision index.</param>
    /// <param name="chunkIndex">The contiguous zero-based chunk index.</param>
    /// <param name="modality">Image, audio, or video.</param>
    /// <param name="mimeType">The exact MIME type of this encoded revision.</param>
    /// <param name="data">The non-empty exact encoded bytes of this chunk.</param>
    /// <param name="isLastChunk">
    /// Whether concatenating this revision's chunks now yields a complete
    /// provisional encoded media value.
    /// </param>
    public GenerationMediaChunkReceived(
        int candidateIndex,
        int itemIndex,
        int revisionIndex,
        int chunkIndex,
        ContentModality modality,
        MediaMimeType mimeType,
        ReadOnlyMemory<byte> data,
        bool isLastChunk)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(candidateIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(itemIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(revisionIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(chunkIndex);
        ArgumentNullException.ThrowIfNull(modality);
        ArgumentNullException.ThrowIfNull(mimeType);

        if (modality != ContentModality.Image
            && modality != ContentModality.Audio
            && modality != ContentModality.Video)
        {
            throw new ArgumentException("A media chunk requires a media modality.", nameof(modality));
        }

        if (data.IsEmpty)
        {
            throw new ArgumentException("A media chunk cannot be empty.", nameof(data));
        }

        CandidateIndex = candidateIndex;
        ItemIndex = itemIndex;
        RevisionIndex = revisionIndex;
        ChunkIndex = chunkIndex;
        Modality = modality;
        MimeType = mimeType;
        _data = data.ToArray();
        IsLastChunk = isLastChunk;
    }

    /// <summary>Gets the zero-based candidate index.</summary>
    public int CandidateIndex { get; }

    /// <summary>Gets the zero-based item index.</summary>
    public int ItemIndex { get; }

    /// <summary>Gets the zero-based revision index.</summary>
    public int RevisionIndex { get; }

    /// <summary>Gets the contiguous zero-based chunk index.</summary>
    public int ChunkIndex { get; }

    /// <summary>Gets the media modality.</summary>
    public ContentModality Modality { get; }

    /// <summary>Gets the exact MIME type of this revision.</summary>
    public MediaMimeType MimeType { get; }

    /// <summary>Gets a copy of this chunk's encoded bytes.</summary>
    public ReadOnlyMemory<byte> Data => _data.ToArray();

    /// <summary>Gets whether the provisional encoded revision is complete.</summary>
    public bool IsLastChunk { get; }

    /// <inheritdoc />
    public bool Equals(GenerationMediaChunkReceived<TResponse>? other) =>
        other is not null
        && CandidateIndex == other.CandidateIndex
        && ItemIndex == other.ItemIndex
        && RevisionIndex == other.RevisionIndex
        && ChunkIndex == other.ChunkIndex
        && Equals(Modality, other.Modality)
        && Equals(MimeType, other.MimeType)
        && IsLastChunk == other.IsLastChunk
        && _data.AsSpan().SequenceEqual(other._data);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(CandidateIndex);
        hash.Add(ItemIndex);
        hash.Add(RevisionIndex);
        hash.Add(ChunkIndex);
        hash.Add(Modality);
        hash.Add(MimeType);
        hash.Add(IsLastChunk);

        foreach (var value in _data)
        {
            hash.Add(value);
        }

        return hash.ToHashCode();
    }
}
