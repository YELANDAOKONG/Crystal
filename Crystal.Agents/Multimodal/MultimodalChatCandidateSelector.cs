using Crystal.Multimodal.Chat;

namespace Crystal.Multimodal.Agents;

/// <summary>Selects one candidate from a multimodal Chat response.</summary>
/// <param name="response">The exact model response.</param>
/// <param name="cancellationToken">A token that cancels selection.</param>
/// <returns>The zero-based selected candidate index.</returns>
public delegate ValueTask<int> MultimodalChatCandidateSelector(
    MultimodalChatResponse response,
    CancellationToken cancellationToken);
