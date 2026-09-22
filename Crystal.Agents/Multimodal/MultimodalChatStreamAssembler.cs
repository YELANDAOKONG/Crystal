using System.Text;

using Crystal.Multimodal.Chat;
using Crystal.Multimodal.Tools;
using Crystal.Reasoning;

namespace Crystal.Multimodal.Agents;

internal sealed class MultimodalChatStreamAssembler
{
    private readonly Dictionary<int, CandidateBuffer> _candidates = [];
    private TokenUsage? _usage;

    public void Apply(MultimodalChatStreamEvent streamEvent)
    {
        ArgumentNullException.ThrowIfNull(streamEvent);

        switch (streamEvent)
        {
            case MultimodalMessageStarted started:
                GetItem(started).StartMessage(started.Role);
                break;
            case MultimodalMessageTextDelta text:
                GetItem(text).AppendMessageText(text.ContentIndex, text.Text);
                break;
            case MultimodalMessageContentReceived content:
                GetItem(content).SetMessageContent(
                    content.ContentIndex,
                    content.Content);
                break;
            case MultimodalReasoningTextDelta reasoning:
                GetItem(reasoning).AppendReasoningText(
                    reasoning.PartIndex,
                    reasoning.Kind,
                    reasoning.Text);
                break;
            case MultimodalReasoningContentReceived reasoning:
                GetItem(reasoning).SetReasoningContent(
                    reasoning.PartIndex,
                    reasoning.Kind,
                    reasoning.Content);
                break;
            case MultimodalReasoningStateReceived state:
                GetItem(state).SetReasoningState(state.State);
                break;
            case MultimodalToolCallDelta toolCall:
                GetItem(toolCall).AppendToolCall(
                    toolCall.CallIdDelta,
                    toolCall.NameDelta,
                    toolCall.ArgumentsDelta);
                break;
            case MultimodalToolCallContentReceived content:
                GetItem(content).SetToolCallContent(
                    content.ContentIndex,
                    content.Content);
                break;
            case MultimodalChatCandidateCompleted completed:
                GetCandidate(completed.CandidateIndex)
                    .Complete(completed.FinishReason);
                break;
            case MultimodalChatUsageReceived usage:
                SetUsage(usage.Usage);
                break;
            default:
                throw new NotSupportedException(
                    $"Unsupported multimodal Chat stream event {streamEvent.GetType().Name}.");
        }
    }

    public MultimodalChatResponse ToResponse()
    {
        if (_candidates.Count == 0)
        {
            throw new InvalidOperationException(
                "The multimodal Chat stream produced no candidates.");
        }

        var candidates = SnapshotContiguous(
            _candidates,
            static candidate => candidate.ToCandidate(),
            "candidate");
        return new MultimodalChatResponse(candidates, _usage);
    }

    private ItemBuffer GetItem(MultimodalChatItemStreamEvent streamEvent) =>
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

    private void SetUsage(TokenUsage usage)
    {
        if (_usage is not null)
        {
            throw new InvalidOperationException(
                "The multimodal Chat stream reported usage more than once.");
        }

        _usage = usage;
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
                    $"The multimodal Chat stream omitted {itemName} index {index}.");
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
                    "The multimodal Chat stream emitted an item after candidate completion.");
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
                    "The multimodal Chat stream completed a candidate more than once.");
            }

            _finishReason = finishReason;
        }

        public MultimodalChatCandidate ToCandidate()
        {
            if (_finishReason is null)
            {
                throw new InvalidOperationException(
                    "The multimodal Chat stream ended before a candidate completed.");
            }

            var items = SnapshotContiguous(
                _items,
                static item => item.ToItem(),
                "candidate item");
            return new MultimodalChatCandidate(items, _finishReason);
        }
    }

    private sealed class ItemBuffer
    {
        private readonly StringBuilder _arguments = new();
        private readonly StringBuilder _callId = new();
        private readonly Dictionary<int, ContentBuffer> _messageContents = [];
        private readonly StringBuilder _name = new();
        private readonly Dictionary<int, ReasoningPartBuffer> _reasoningParts = [];
        private readonly Dictionary<int, MultimodalContent> _toolCallContents = [];
        private ItemKind _kind;
        private MultimodalChatRole? _messageRole;
        private bool _messageStarted;
        private OpaqueReasoningState? _reasoningState;

        public void StartMessage(MultimodalChatRole role)
        {
            SetKind(ItemKind.Message);
            if (_messageStarted)
            {
                throw new InvalidOperationException(
                    "The multimodal Chat stream started a message more than once.");
            }

            _messageStarted = true;
            _messageRole = role;
        }

        public void AppendMessageText(int contentIndex, string text)
        {
            RequireMessageStart();
            GetMessageContent(contentIndex).AppendText(text);
        }

        public void SetMessageContent(
            int contentIndex,
            MultimodalContent content)
        {
            RequireMessageStart();
            GetMessageContent(contentIndex).SetContent(content);
        }

        public void AppendReasoningText(
            int partIndex,
            MultimodalReasoningKind kind,
            string text)
        {
            SetKind(ItemKind.Reasoning);
            GetReasoningPart(partIndex, kind).AppendText(text);
        }

        public void SetReasoningContent(
            int partIndex,
            MultimodalReasoningKind kind,
            MultimodalContent content)
        {
            SetKind(ItemKind.Reasoning);
            GetReasoningPart(partIndex, kind).SetContent(content);
        }

        public void SetReasoningState(OpaqueReasoningState state)
        {
            SetKind(ItemKind.Reasoning);
            if (_reasoningState is not null)
            {
                throw new InvalidOperationException(
                    "A streamed reasoning item received opaque state more than once.");
            }

            _reasoningState = state;
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

        public void SetToolCallContent(
            int contentIndex,
            MultimodalContent content)
        {
            SetKind(ItemKind.ToolCall);
            if (!_toolCallContents.TryAdd(contentIndex, content))
            {
                throw new InvalidOperationException(
                    "The multimodal Chat stream reported tool-call content more than once.");
            }
        }

        public MultimodalChatItem ToItem() =>
            _kind switch
            {
                ItemKind.Message => ToMessage(),
                ItemKind.Reasoning => ToReasoningItem(),
                ItemKind.ToolCall => ToToolCall(),
                _ => throw new InvalidOperationException(
                    "A streamed multimodal Chat item received no content.")
            };

        private MultimodalMessage ToMessage()
        {
            RequireMessageStart();
            var contents = SnapshotContiguous(
                _messageContents,
                static content => content.ToContent(),
                "message content");
            return new MultimodalMessage(_messageRole!, contents);
        }

        private MultimodalReasoningItem ToReasoningItem()
        {
            var parts = SnapshotContiguous(
                _reasoningParts,
                static part => part.ToPart(),
                "reasoning part");
            return new MultimodalReasoningItem(
                new MultimodalReasoningContent(parts, _reasoningState));
        }

        private MultimodalToolCall ToToolCall()
        {
            if (string.IsNullOrWhiteSpace(_callId.ToString())
                || string.IsNullOrWhiteSpace(_name.ToString()))
            {
                throw new InvalidOperationException(
                    "The multimodal Chat stream ended with an incomplete tool call.");
            }

            var contents = SnapshotContiguous(
                _toolCallContents,
                static content => content,
                "tool-call content");
            return new MultimodalToolCall(
                _callId.ToString(),
                _name.ToString(),
                _arguments.ToString(),
                contents);
        }

        private ContentBuffer GetMessageContent(int contentIndex)
        {
            if (_messageContents.TryGetValue(contentIndex, out var content))
            {
                return content;
            }

            content = new ContentBuffer();
            _messageContents[contentIndex] = content;
            return content;
        }

        private ReasoningPartBuffer GetReasoningPart(
            int partIndex,
            MultimodalReasoningKind kind)
        {
            if (_reasoningParts.TryGetValue(partIndex, out var part))
            {
                part.SetKind(kind);
                return part;
            }

            part = new ReasoningPartBuffer(kind);
            _reasoningParts[partIndex] = part;
            return part;
        }

        private void RequireMessageStart()
        {
            SetKind(ItemKind.Message);
            if (!_messageStarted)
            {
                throw new InvalidOperationException(
                    "The multimodal Chat stream emitted message content before starting the message.");
            }
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
                    "A streamed multimodal Chat item mixed incompatible event types.");
            }
        }
    }

    private sealed class ContentBuffer
    {
        private readonly StringBuilder _text = new();
        private MultimodalContent? _content;
        private bool _receivedTextDelta;

        public void AppendText(string text)
        {
            if (_content is not null)
            {
                throw new InvalidOperationException(
                    "A streamed content block mixed text deltas with complete content.");
            }

            _receivedTextDelta = true;
            _text.Append(text);
        }

        public void SetContent(MultimodalContent content)
        {
            if (_content is not null || _receivedTextDelta)
            {
                throw new InvalidOperationException(
                    "A streamed content block was completed more than once.");
            }

            _content = content;
        }

        public MultimodalContent ToContent() =>
            _content
            ?? (_receivedTextDelta
                ? new TextContent(_text.ToString())
                : throw new InvalidOperationException(
                    "A streamed content block received no content."));
    }

    private sealed class ReasoningPartBuffer
    {
        private readonly ContentBuffer _content = new();
        private MultimodalReasoningKind _kind;

        public ReasoningPartBuffer(MultimodalReasoningKind kind) =>
            _kind = kind;

        public void AppendText(string text) => _content.AppendText(text);

        public void SetContent(MultimodalContent content) =>
            _content.SetContent(content);

        public void SetKind(MultimodalReasoningKind kind)
        {
            if (_kind != kind)
            {
                throw new InvalidOperationException(
                    "A streamed reasoning part changed its classification.");
            }
        }

        public MultimodalReasoningPart ToPart() =>
            new(_content.ToContent(), _kind);
    }

    private enum ItemKind
    {
        None,
        Message,
        Reasoning,
        ToolCall
    }
}
