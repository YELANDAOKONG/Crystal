namespace Crystal.Internal;

internal sealed class TokenUsageAccumulator
{
    private bool _hasCompleteUsage = true;
    private bool _hasCompleteReasoningUsage = true;
    private bool _hasUsage;
    private long _inputTokenCount;
    private long _outputTokenCount;
    private long _reasoningTokenCount;

    public void Add(TokenUsage? usage)
    {
        if (usage is null)
        {
            _hasCompleteUsage = false;
            return;
        }

        _hasUsage = true;
        _inputTokenCount = checked(
            _inputTokenCount + usage.InputTokenCount);
        _outputTokenCount = checked(
            _outputTokenCount + usage.OutputTokenCount);

        if (usage.ReasoningTokenCount is long reasoningTokenCount)
        {
            _reasoningTokenCount = checked(
                _reasoningTokenCount + reasoningTokenCount);
        }
        else
        {
            _hasCompleteReasoningUsage = false;
        }
    }

    public TokenUsage? Build()
    {
        if (!_hasUsage || !_hasCompleteUsage)
        {
            return null;
        }

        return new TokenUsage(
            _inputTokenCount,
            _outputTokenCount,
            _hasCompleteReasoningUsage ? _reasoningTokenCount : null);
    }
}
