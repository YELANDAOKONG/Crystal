using Crystal.Chat;

namespace Crystal.Agents;

/// <summary>
/// Selects one candidate from a complete model response.
/// </summary>
/// <param name="response">The exact model response.</param>
/// <param name="cancellationToken">
/// A token that cancels candidate selection.
/// </param>
/// <returns>The zero-based selected candidate index.</returns>
public delegate ValueTask<int> ChatCandidateSelector(
    ChatResponse response,
    CancellationToken cancellationToken);
