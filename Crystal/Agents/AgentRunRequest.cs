using Crystal.Chat;
using Crystal.Internal;
using Crystal.Reasoning;

namespace Crystal.Agents;

/// <summary>
/// Contains one caller-authored Agent run request.
/// </summary>
public sealed record AgentRunRequest
{
    /// <summary>
    /// Initializes an Agent run request.
    /// </summary>
    /// <param name="runId">The caller-supplied non-empty run identifier.</param>
    /// <param name="items">The exact ordered initial transcript.</param>
    /// <param name="limits">The finite run limits.</param>
    /// <param name="reasoning">Optional portable reasoning hints.</param>
    public AgentRunRequest(
        Guid runId,
        IEnumerable<ChatItem> items,
        AgentRunLimits limits,
        ReasoningOptions? reasoning = null)
    {
        if (runId == Guid.Empty)
        {
            throw new ArgumentException(
                "Run identifier cannot be empty.",
                nameof(runId));
        }

        ArgumentNullException.ThrowIfNull(limits, nameof(limits));

        RunId = runId;
        Items = CollectionSnapshot.Create(items, nameof(items));
        Limits = limits;
        Reasoning = reasoning;
    }

    /// <summary>
    /// Gets the caller-supplied run identifier.
    /// </summary>
    public Guid RunId { get; }

    /// <summary>
    /// Gets the exact ordered initial transcript.
    /// </summary>
    public IReadOnlyList<ChatItem> Items { get; }

    /// <summary>
    /// Gets the finite run limits.
    /// </summary>
    public AgentRunLimits Limits { get; }

    /// <summary>
    /// Gets optional portable reasoning hints.
    /// </summary>
    public ReasoningOptions? Reasoning { get; }
}
