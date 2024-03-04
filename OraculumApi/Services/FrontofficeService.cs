using System.Text;
using System.Threading.Channels;
using Newtonsoft.Json;
using Oraculum;
using OraculumApi.Models;
using OraculumApi.Models.BackOffice;
using OraculumApi.Models.FrontOffice;

/*
    This class is the result of a refactoring to FrontOfficeController.
    Please, refer to the original file for the original code history.
*/
public class FrontofficeService
{
    private readonly IConfiguration _configuration;
    private readonly SibyllaManager _sibyllaManager;
    private readonly BaseService<SibyllaPersistentConfig> _sibyllaConfigService;
    private readonly ChatDetailService _chatDetailService;
    private readonly BaseService<Message> _messageService;
    public FrontofficeService(IConfiguration configuration, SibyllaManager sibyllaManager, BaseService<SibyllaPersistentConfig> sibyllaConfigService, ChatDetailService chatDetailService, BaseService<Message> messageService)
    {
        _configuration = configuration;
        _sibyllaManager = sibyllaManager;
        _sibyllaConfigService = sibyllaConfigService;
        _chatDetailService = chatDetailService;
        _messageService = messageService;
    }

    public async Task<(Sibylla, string)> ConnectSibylla(HttpContext httpContext, string sibyllaName, string? sibyllaRef = null)
    {
        if (sibyllaRef == null)
        {
            var sibyllaPersistentConfig = await _sibyllaConfigService.GetByProperty<string>("name", sibyllaName);
            var sibyllaConf = sibyllaPersistentConfig.Count() > 0 ? SibyllaConf.FromJson(sibyllaPersistentConfig.First().configJSON!) : null;
            if (sibyllaConf == null && _configuration.GetSection("DBSibyllaeConfigOnly").Get<bool>() == true)
                throw new Exception("Sibylla configuration not found in the database and DBSibyllaeConfigOnly is set to true.");

            var (id, _) = await _sibyllaManager.AddSibylla(sibyllaName, sibyllaConf: sibyllaConf, expiration: DateTime.Now.AddMinutes(60));
            httpContext.Session.SetString("sibyllaRef", id.ToString());
            sibyllaRef = id.ToString();
        }
        var sibylla = _sibyllaManager.GetSibylla(sibyllaName, Guid.Parse((string)sibyllaRef));
        return (sibylla, sibyllaRef);
    }

    public async Task<List<SibyllaInfoDto>?> GetAllSibyllaeInfo()
    {
        var sibyllaeFromDB = await _sibyllaConfigService.List();
        var sibyllae = sibyllaeFromDB.Select(s =>
        {
            var sibyllaConf = SibyllaConf.FromJson(s.configJSON!);
            return new SibyllaInfoDto
            {
                Id = s.name,
                Title = sibyllaConf?.Title,
                BaseAssistantPrompt = sibyllaConf?.BaseAssistantPrompt,
                Hidden = sibyllaConf?.Hidden ?? false
            };
        }).ToList();

        if (_configuration.GetSection("DBSibyllaeConfigOnly").Get<bool>() != true)
        {
            sibyllae.AddRange(_sibyllaManager.GetSibyllaeDict().Select(s => new SibyllaInfoDto
            {
                Id = s.Key,
                Title = s.Value.Title,
                BaseAssistantPrompt = s.Value.BaseAssistantPrompt,
                Hidden = s.Value.Hidden
            }));
        }

        return sibyllae;
    }

    public async Task<ChatDetailDTO?> CreateChat(HttpContext httpContext, string sibyllaId)
    {
        var (_, SibyllaRef) = await ConnectSibylla(httpContext, sibyllaId);

        var chatDetailGuid = await _chatDetailService.Add(new ChatDetail
        {
            sibyllaId = sibyllaId,
            sibyllaRef = SibyllaRef,
            creationTimestamp = DateTime.Now
        });

        var chatDetail = await _chatDetailService.Get(chatDetailGuid.Value);
        var chatDetailDTO = chatDetail?.toDTO();

        chatDetailDTO!.IsActive = _sibyllaManager.IsSibyllaActive(sibyllaId, Guid.Parse(chatDetail!.sibyllaRef!));

        return chatDetailDTO;
    }

    public async Task<ChatDetailDTO?> GetChatById(string sibyllaId, string chatId)
    {
        var chatDetail = await _chatDetailService.Get(Guid.Parse(chatId));

        if (chatDetail == null || chatDetail.sibyllaId != sibyllaId) return null;

        var chatDetailDTO = chatDetail.toDTO();

        var messages = await _chatDetailService.GetEnrichedMessages(chatDetailDTO.MessagesIds);

        chatDetailDTO.Messages = messages?.Select(m => m.toDTO()).ToList();
        chatDetailDTO.IsActive = _sibyllaManager.IsSibyllaActive(sibyllaId, Guid.Parse(chatDetail.sibyllaRef!));

        return chatDetailDTO;
    }

    public async Task<PushStreamResult> AddMessageAndStreamAnswer(string sibyllaId, string chatId, MessageDTO message)
    {
        if (message.Text == null || chatId == null)
            throw new HttpResponseException(400, "Message must have text and chatId must be provided");

        var sendTime = DateTime.Now;

        var chatDetail = await _chatDetailService.Get(Guid.Parse(chatId));

        if (chatDetail == null || chatDetail.sibyllaId != sibyllaId) throw new HttpResponseException(400, "Invalid chat");

        var Sibylla = _sibyllaManager.GetSibylla(chatDetail.sibyllaId!, Guid.Parse(chatDetail.sibyllaRef!));
        var answerid = Guid.NewGuid().ToString();

        var channel = Channel.CreateUnbounded<string>();

        _ = WriteToChannel(Sibylla, message.Text, answerid, channel.Writer);

        var sibyllaAnswerText = new StringBuilder();

        // Stream the response as Server-Sent Events
        return new PushStreamResult(
            async (stream, _, cancellationToken) =>
            {
                var userMessageId = await _messageService.Add(new Message
                {
                    sender = "user",
                    text = message.Text,
                    timestamp = sendTime
                });

                var assistantMessageId = await _messageService.Add(new Message { });

#pragma warning disable CS8619

                chatDetail.messagesIds = chatDetail.messagesIds?
                    .Append(userMessageId.ToString())
                    .Append(assistantMessageId.ToString()).ToArray()
                    ?? new string[] { userMessageId.ToString()!, assistantMessageId.ToString()! };

#pragma warning restore CS8619

                await _chatDetailService.Update(chatDetail);

                var writer = new StreamWriter(stream);
                await foreach (var chunk in channel.Reader.ReadAllAsync(cancellationToken))
                {
                    if (!string.IsNullOrEmpty(chunk))
                    {
                        sibyllaAnswerText.Append(chunk);
                        var messageChunk = new
                        {
                            UserMessageId = userMessageId,
                            AssistantMessageId = assistantMessageId,
                            Delta = new
                            {
                                Content = chunk,
                            },
                        };
                        string jsonChunk = JsonConvert.SerializeObject(messageChunk);
                        await writer.WriteAsync(jsonChunk);
                        await writer.FlushAsync();
                    }
                }

                await _messageService.Update(new Message
                {
                    id = Guid.Parse(assistantMessageId.ToString()!),
                    sender = "assistant",
                    text = sibyllaAnswerText.ToString(),
                    timestamp = DateTime.Now
                });

            }, "text/event-stream");
    }

    public async Task<(
        string? answer, 
        string? prompt, 
        Guid? userMessageId, 
        Guid? assistantMessageId, 
        List<OraculumApi.Models.BackOffice.Fact> usedFactsList, 
        List<OraculumApi.Models.BackOffice.Fact> extraFactsList
        )> AddMessageAndAnswerExplain(string sibyllaId, string chatId, MessageDTO message)
        {
            if (message.Text == null || chatId == null)
            throw new HttpResponseException(400, "Message must have text and chatId must be provided");

        var sendTime = DateTime.Now;

        var chatDetail = await _chatDetailService.Get(Guid.Parse(chatId));

        if (chatDetail == null || chatDetail.sibyllaId != sibyllaId) throw new HttpResponseException(400, "Invalid chat");

        var Sibylla = _sibyllaManager.GetSibylla(chatDetail.sibyllaId!, Guid.Parse(chatDetail.sibyllaRef!));

        var answer = await Sibylla.Answer(message.Text);
        var prompt = Sibylla.Configuration.BaseSystemPrompt;
        var usedFacts = await _sibyllaManager.FindRelevantFacts(
                message.Text,
                factTypeFilter: Sibylla.Configuration.MemoryConfiguration.FactFilter,
                categoryFilter: Sibylla.Configuration.MemoryConfiguration.CategoryFilter,
                tagsFilter: Sibylla.Configuration.MemoryConfiguration.TagFilter,
                limit: Sibylla.Configuration.MemoryConfiguration.Limit,
                autoCutPercentage: Sibylla.Configuration.MemoryConfiguration.AutoCutPercentage
            );
        var usedFactsList = usedFacts.Select(fact => OraculumApi.Models.BackOffice.Fact.FromOraculumFact(fact)).ToList();
        var extraFacts = await _sibyllaManager.FindRelevantFacts(
                message.Text,
                factTypeFilter: Sibylla.Configuration.MemoryConfiguration.FactFilter,
                categoryFilter: Sibylla.Configuration.MemoryConfiguration.CategoryFilter,
                tagsFilter: Sibylla.Configuration.MemoryConfiguration.TagFilter,
                limit: Sibylla.Configuration.MemoryConfiguration.Limit
            );
        var extraFactsList = extraFacts.Select(fact => OraculumApi.Models.BackOffice.Fact.FromOraculumFact(fact)).ToList();

        // Remove facts with IDs in usedFactsList
        extraFactsList.RemoveAll(fact => usedFactsList.Any(usedFact => usedFact.Id == fact.Id));

        var userMessageId = await _messageService.Add(new Message
        {
            sender = "user",
            text = message.Text,
            timestamp = sendTime
        });

#pragma warning disable CS8619

        var assistantMessageId = await _messageService.Add(new Message
        {
            sender = "assistant",
            text = answer,
            timestamp = DateTime.Now,
            factIds = usedFactsList.Select(f => f.Id.ToString()).ToArray(),
            extraFactIds = extraFactsList.Select(f => f.Id.ToString()).ToArray()
        });

        chatDetail.messagesIds = chatDetail.messagesIds?
            .Append(userMessageId.ToString())
            .Append(assistantMessageId.ToString()).ToArray()
            ?? new string[] { userMessageId.ToString()!, assistantMessageId.ToString()! };

#pragma warning restore CS8619

        await _chatDetailService.Update(chatDetail);

        return (answer, prompt, userMessageId, assistantMessageId, usedFactsList, extraFactsList);
    }

    private async Task WriteToChannel(Sibylla sibylla, string question, string answerid, ChannelWriter<string> writer)
    {
        await foreach (var fragment in sibylla.AnswerAsync(question))
        {
            await writer.WriteAsync(fragment);
        }
        writer.TryComplete();
    }
}