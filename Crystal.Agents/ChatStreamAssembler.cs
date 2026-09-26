using System.Text;

using Crystal.Chat;
using Crystal.Reasoning;
using Crystal.Tools;

namespace Crystal.Agents;

internal sealed class ChatStreamAssembler
{
    private readonly Dictionary<int, CandidateBuffer> _candidates = [];
    private TokenUsage? _usage;

    public void Apply(ChatStreamEvent streamEvent)
    {
        ArgumentNullException.ThrowIfNull(streamEvent);

        switch (streamEvent)
        {
            case ChatTextDelta text:
                GetItem(text).AppendText(text.Role, text.Text);
                break;
            case ChatReasoningTextDelta reasoning:
                GetItem(reasoning).AppendReasoningText(
                    reasoning.TextSegmentIndex,
                    reasoning.Kind,
                    reasoning.Text);
                break;
            case ChatReasoningStateReceived state:
                GetItem(state).SetReasoningState(state.State);
                break;
            case ChatToolCallDelta toolCall:
                GetItem(toolCall).AppendToolCall(
                    toolCall.CallIdDelta,
                    toolCall.NameDelta,
                    toolCall.ArgumentsDelta);
                break;
            case ChatCandidateCompleted completed:
                GetCandidate(completed.CandidateIndex)
                    .Complete(completed.FinishReason);
                break;
            case ChatUsageReceived usage:
                if (_usage is not null)
                {
                    throw new InvalidOperationException(
                        "The Chat stream reported usage more than once.");
                }

                _usage = usage.Usage;
                break;
            default:
                throw new NotSupportedException(
                    $"Unsupported Chat stream event {streamEvent.GetType().Name}.");
        }
    }

    public ChatResponse ToResponse()
    {
        if (_candidates.Count == 0)
        {
            throw new InvalidOperationException(
                "The Chat stream produced no candidates.");
        }

        return new ChatResponse(
            SnapshotContiguous(
                _candidates,
                static candidate => candidate.ToCandidate(),
                "candidate"),
            _usage);
    }

    private ItemBuffer GetItem(ChatItemStreamEvent streamEvent) =>
        GetCandidate(streamEvent.CandidateIndex).GetItem(streamEvent.ItemIndex);

    private CandidateBuffer GetCandidate(int candidateIndex)
    {
        if (_candidates.TryGetValue(candidateIndex, out var candidate))
        {
            return candidate;
        }

        candidate = new CandidateBuffer();
        _candidates[candidateIndex] = candidate;
        return candidate;
    }

    private static IReadOnlyList<TOutput> SnapshotContiguous<TBuffer, TOutput>(
        IReadOnlyDictionary<int, TBuffer> buffers,
        Func<TBuffer, TOutput> create,
        string itemName)
    {
        var items = new TOutput[buffers.Count];
        for (var index = 0; index < items.Length; index++)
        {
            if (!buffers.TryGetValue(index, out var buffer))
            {
                throw new InvalidOperationException(
                    $"The Chat stream omitted {itemName} index {index}.");
            }

            items[index] = create(buffer);
        }

        return Array.AsReadOnly(items);
    }

    private sealed class CandidateBuffer
    {
        private readonly Dictionary<int, ItemBuffer> _items = [];
        private FinishReason? _finishReason;

        public ItemBuffer GetItem(int itemIndex)
        {
            if (_finishReason is not null)
            {
                throw new InvalidOperationException(
                    "The Chat stream emitted an item after candidate completion.");
            }

            if (_items.TryGetValue(itemIndex, out var item))
            {
                return item;
            }

            item = new ItemBuffer();
            _items[itemIndex] = item;
            return item;
        }

        public void Complete(FinishReason finishReason)
        {
            if (_finishReason is not null)
            {
                throw new InvalidOperationException(
                    "The Chat stream completed a candidate more than once.");
            }

            _finishReason = finishReason;
        }

        public ChatCandidate ToCandidate()
        {
            if (_finishReason is null)
            {
                throw new InvalidOperationException(
                    "The Chat stream ended before a candidate completed.");
            }

            return new ChatCandidate(
                SnapshotContiguous(
                    _items,
                    static item => item.ToItem(),
                    "candidate item"),
                _finishReason);
        }
    }

    private sealed class ItemBuffer
    {
        private readonly StringBuilder _arguments = new();
        private readonly StringBuilder _callId = new();
        private readonly StringBuilder _messageText = new();
        private readonly StringBuilder _name = new();
        private readonly Dictionary<int, ReasoningSegmentBuffer> _segments = [];
        private ItemKind _kind;
        private ChatRole? _role;
        private OpaqueReasoningState? _state;

        public void AppendText(ChatRole role, string text)
        {
            SetKind(ItemKind.Message);
            if (_role is not null && _role != role)
            {
                throw new InvalidOperationException(
                    "A streamed Chat message changed its role.");
            }

            _role = role;
            _messageText.Append(text);
        }

        public void AppendReasoningText(
            int segmentIndex,
            ReasoningTextKind kind,
            string text)
        {
            SetKind(ItemKind.Reasoning);
            if (!_segments.TryGetValue(segmentIndex, out var segment))
            {
                segment = new ReasoningSegmentBuffer(kind);
                _segments[segmentIndex] = segment;
            }

            segment.Append(kind, text);
        }

        public void SetReasoningState(OpaqueReasoningState state)
        {
            SetKind(ItemKind.Reasoning);
            if (_state is not null)
            {
                throw new InvalidOperationException(
                    "The Chat stream reported reasoning state more than once.");
            }

            _state = state;
        }

        public void AppendToolCall(
            string callIdDelta,
            string nameDelta,
            string argumentsDelta)
        {
            SetKind(ItemKind.ToolCall);
            _callId.Append(callIdDelta);
            _name.Append(nameDelta);
            _arguments.Append(argumentsDelta);
        }

        public ChatItem ToItem() =>
            _kind switch
            {
                ItemKind.Message => new ChatMessage(_role!, _messageText.ToString()),
                ItemKind.Reasoning => new ChatReasoningItem(
                    new ReasoningContent(
                        SnapshotContiguous(
                            _segments,
                            static segment => segment.ToReasoningText(),
                            "reasoning text segment"),
                        _state)),
                ItemKind.ToolCall => ToToolCall(),
                _ => throw new InvalidOperationException(
                    "A streamed Chat item received no content.")
            };

        private ToolCall ToToolCall()
        {
            if (string.IsNullOrWhiteSpace(_callId.ToString())
                || string.IsNullOrWhiteSpace(_name.ToString()))
            {
                throw new InvalidOperationException(
                    "The Chat stream ended with an incomplete tool call.");
            }

            return new ToolCall(
                _callId.ToString(),
                _name.ToString(),
                _arguments.ToString());
        }

        private void SetKind(ItemKind kind)
        {
            if (_kind == ItemKind.None)
            {
                _kind = kind;
                return;
            }

            if (_kind != kind)
            {
                throw new InvalidOperationException(
                    "A streamed Chat item mixed incompatible event types.");
            }
        }
    }

    private sealed class ReasoningSegmentBuffer
    {
        private readonly ReasoningTextKind _kind;
        private readonly StringBuilder _text = new();

        public ReasoningSegmentBuffer(ReasoningTextKind kind) => _kind = kind;

        public void Append(ReasoningTextKind kind, string text)
        {
            if (_kind != kind)
            {
                throw new InvalidOperationException(
                    "A streamed reasoning text segment changed its classification.");
            }

            _text.Append(text);
        }

        public ReasoningText ToReasoningText()
        {
            if (_text.Length == 0)
            {
                throw new InvalidOperationException(
                    "A streamed reasoning text segment received no text.");
            }

            return new ReasoningText(_text.ToString(), _kind);
        }
    }

    private enum ItemKind
    {
        None,
        Message,
        Reasoning,
        ToolCall
    }
}
