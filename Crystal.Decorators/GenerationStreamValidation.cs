using System.Runtime.CompilerServices;

using Crystal.Generation.Streaming;
using Crystal.Media;
using Crystal.Pipelines;

namespace Crystal.Decorators;

/// <summary>Provides opt-in checks for generated-media stream ordering.</summary>
public static class GenerationStreamValidation
{
    /// <summary>
    /// Requires a successful stream to contain valid provisional revisions and
    /// exactly one final completion event.
    /// </summary>
    /// <typeparam name="TRequest">The target-specific request type.</typeparam>
    /// <typeparam name="TResponse">The target-specific response type.</typeparam>
    /// <returns>Middleware that forwards valid events unchanged.</returns>
    public static StreamingMiddleware<TRequest, GenerationStreamEvent<TResponse>>
        RequireProtocol<TRequest, TResponse>()
        where TResponse : class =>
        next =>
        {
            ArgumentNullException.ThrowIfNull(next);
            return (request, cancellationToken) =>
                ValidateAsync(next, request, cancellationToken);
        };

    private static async IAsyncEnumerable<GenerationStreamEvent<TResponse>>
        ValidateAsync<TRequest, TResponse>(
            StreamingOperation<TRequest, GenerationStreamEvent<TResponse>> next,
            TRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        where TResponse : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        var source = next(request, cancellationToken)
            ?? throw new InvalidOperationException(
                "The generation client returned no stream.");
        var items = new Dictionary<(int Candidate, int Item), RevisionState>();
        GenerationStreamCompleted<TResponse>? completed = null;

        await foreach (var streamEvent in source
            .WithCancellation(cancellationToken)
            .ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (completed is not null)
            {
                throw new InvalidOperationException(
                    "The generation stream continued after completion.");
            }

            switch (streamEvent)
            {
                case GenerationMediaPreviewReceived<TResponse> preview:
                    GetState(items, preview.CandidateIndex, preview.ItemIndex)
                        .AcceptPreview(preview.RevisionIndex);
                    yield return preview;
                    break;

                case GenerationMediaChunkReceived<TResponse> chunk:
                    GetState(items, chunk.CandidateIndex, chunk.ItemIndex)
                        .AcceptChunk(chunk);
                    yield return chunk;
                    break;

                case GenerationStreamCompleted<TResponse> terminal:
                    if (items.Values.Any(static state => !state.IsComplete))
                    {
                        throw new InvalidOperationException(
                            "The generation stream ended with an incomplete media revision.");
                    }

                    completed = terminal;
                    break;

                default:
                    throw new InvalidOperationException(
                        "The generation stream returned an unknown event.");
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (completed is null)
        {
            throw new InvalidOperationException(
                "The generation stream ended without completion.");
        }

        yield return completed;
    }

    private static RevisionState GetState(
        Dictionary<(int Candidate, int Item), RevisionState> items,
        int candidateIndex,
        int itemIndex)
    {
        var key = (candidateIndex, itemIndex);
        if (!items.TryGetValue(key, out var state))
        {
            state = new RevisionState();
            items.Add(key, state);
        }

        return state;
    }

    private sealed class RevisionState
    {
        private int _revisionIndex = -1;
        private int _nextChunkIndex;
        private bool _usesChunks;
        private ContentModality? _modality;
        private MediaMimeType? _mimeType;

        public bool IsComplete { get; private set; } = true;

        public void AcceptPreview(int revisionIndex)
        {
            BeginRevision(revisionIndex);
            _usesChunks = false;
            IsComplete = true;
        }

        public void AcceptChunk<TResponse>(
            GenerationMediaChunkReceived<TResponse> chunk)
            where TResponse : class
        {
            if (chunk.RevisionIndex > _revisionIndex)
            {
                BeginRevision(chunk.RevisionIndex);
                _usesChunks = true;
                _modality = chunk.Modality;
                _mimeType = chunk.MimeType;
                _nextChunkIndex = 0;
            }

            if (chunk.RevisionIndex != _revisionIndex
                || !_usesChunks
                || IsComplete
                || chunk.ChunkIndex != _nextChunkIndex
                || chunk.Modality != _modality
                || chunk.MimeType != _mimeType)
            {
                throw new InvalidOperationException(
                    "The generation stream contains an invalid media chunk sequence.");
            }

            _nextChunkIndex = checked(_nextChunkIndex + 1);
            IsComplete = chunk.IsLastChunk;
        }

        private void BeginRevision(int revisionIndex)
        {
            if (!IsComplete || revisionIndex <= _revisionIndex)
            {
                throw new InvalidOperationException(
                    "The generation stream contains an invalid media revision.");
            }

            _revisionIndex = revisionIndex;
            IsComplete = false;
        }
    }
}
