using Crystal.Reasoning;

namespace Crystal.Completions;

/// <summary>
/// Contains one caller-authored text-completion request.
/// </summary>
public sealed record CompletionRequest
{
    /// <summary>
    /// Initializes a completion request.
    /// </summary>
    /// <param name="prompt">The exact caller-authored prompt.</param>
    /// <param name="reasoning">Optional portable reasoning hints.</param>
    public CompletionRequest(
        string prompt,
        ReasoningOptions? reasoning = null)
    {
        ArgumentNullException.ThrowIfNull(prompt, nameof(prompt));

        Prompt = prompt;
        Reasoning = reasoning;
    }

    /// <summary>
    /// Gets the exact prompt.
    /// </summary>
    public string Prompt { get; }

    /// <summary>
    /// Gets optional reasoning hints.
    /// </summary>
    public ReasoningOptions? Reasoning { get; }
}
