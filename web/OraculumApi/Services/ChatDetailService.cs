using OraculumApi.Models;
using OraculumApi.Models.FrontOffice;

public class ChatDetailService : BaseService<ChatDetail>, IService<ChatDetail>
{
    private readonly BaseService<Message> _messageService;

    public ChatDetailService(ChatDetailRepository repository, BaseService<Message> messageService) : base(repository)
    {
        _messageService = messageService;
    }

    public async Task<List<Message>> GetEnrichedMessages(string[]? messagesIds)
    {
        var enrichedMessages = new List<Message>();

        if (messagesIds == null) return enrichedMessages;

        foreach (var messageId in messagesIds)
        {
            var message = await _messageService.Get(Guid.Parse(messageId));

            if (message == null)
            {
                enrichedMessages.Add(new Message
                {
                    id = Guid.Parse(messageId)
                });
                continue;
            }

            enrichedMessages.Add(new Message
            {
                id = message.id,
                sender = message.sender,
                text = message.text,
                factIds = message.factIds,
                extraFactIds = message.extraFactIds,
                timestamp = message.timestamp
            });
        }

        return enrichedMessages;
    }
}