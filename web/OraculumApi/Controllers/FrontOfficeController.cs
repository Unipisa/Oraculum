using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using OraculumApi.Attributes;
using OraculumApi.Models.FrontOffice;
using Oraculum;
using System.Text;
using System.Threading.Channels;
using Asp.Versioning;
using OraculumApi.Models.BackOffice;
using OraculumApi.Models;

namespace OraculumApi.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1")]
[ApiVersion("2")]
public class FrontOfficeController : Controller
{

    private readonly ILogger<FrontOfficeController> _logger;
    private readonly SibyllaManager _sibyllaManager;
    private readonly IConfiguration _configuration;
    private ChatDetailService _chatDetailService;
    private BaseService<Message> _messageService;
    private BaseService<Feedback> _feedbackService;
    private BaseService<SibyllaPersistentConfig> _sibyllaConfigService;
    private FrontofficeService _frontofficeService;

    public FrontOfficeController(ILogger<FrontOfficeController> logger, SibyllaManager sibyllaManager, IConfiguration configuration, ChatDetailService chatDetailService, BaseService<Message> messageService, BaseService<Feedback> feedbackService, BaseService<SibyllaPersistentConfig> sibyllaConfigService)
    {
        _logger = logger;
        _sibyllaManager = sibyllaManager;
        _configuration = configuration;
        _chatDetailService = chatDetailService;
        _messageService = messageService;
        _feedbackService = feedbackService;
        _sibyllaConfigService = sibyllaConfigService;
        _frontofficeService = new FrontofficeService(configuration, sibyllaManager, sibyllaConfigService, chatDetailService, messageService);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult ClearSession()
    {
        HttpContext.Session.Clear();
        return Ok();
    }

    [HttpDelete]
    [Route("sibylla/{sibyllaId}/chat/{chatId}")]
    [ValidateModelState]
    [DynamicAuthorize("frontoffice")]
    [SwaggerOperation("DeleteChatsChatId")]
    public virtual IActionResult DeleteChatsChatId([FromRoute][Required] string chatId, [FromRoute][Required] string sibyllaId)
    {
        throw new NotImplementedException();
    }

    [HttpGet]
    [Route("sibylla")]
    [DynamicAuthorize("frontoffice")]
    public async Task<ActionResult<SibyllaInfoDto>> GetAllSibyllae()
    {
        var sibyllae = await _frontofficeService.GetAllSibyllaeInfo();
        return Ok(sibyllae);
    }

    [HttpGet]
    [Route("sibylla/{sibyllaId}/chat")]
    [DynamicAuthorize("frontoffice")]
    public async Task<IActionResult> GetChats([FromRoute] string sibyllaId)
    {
        var chatDetails = await _chatDetailService.GetByProperty<string>("sibyllaId", sibyllaId);

        if (chatDetails == null) return Ok(new List<ChatDetailDTO>());

        var chatDetailDTOs = chatDetails.Select(chatDetail => new ChatDetailDTO
        {
            Id = chatDetail.id,
            SibyllaId = chatDetail.sibyllaId,
            CreationTimestamp = chatDetail.creationTimestamp,
            IsActive = _sibyllaManager.IsSibyllaActive(sibyllaId, Guid.Parse(chatDetail.sibyllaRef!))
        }).ToList();

        return Ok(chatDetailDTOs);
    }

    [HttpPost]
    [Route("sibylla/{sibyllaId}/chat")]
    [DynamicAuthorize("frontoffice")]
    public async Task<IActionResult> PostChat([FromRoute] string sibyllaId)
    {
        if (sibyllaId == null)
            return BadRequest();
            
        var chatDetailDTO = await _frontofficeService.CreateChat(HttpContext, sibyllaId);
        return Ok(chatDetailDTO);
    }

    [HttpGet]
    [Route("sibylla/{sibyllaId}/chat/{chatId}")]
    [DynamicAuthorize("frontoffice")]
    public async Task<IActionResult> GetChatById([FromRoute] string sibyllaId, [FromRoute] string chatId)
    {
        var chatDetailDTO = await _frontofficeService.GetChatById(sibyllaId, chatId);

        if (chatDetailDTO == null) return NotFound();

        return Ok(chatDetailDTO);
    }

    [HttpPost]
    [Route("sibylla/{sibyllaId}/chat/{chatId}/message")]
    [DynamicAuthorize("frontoffice")]
    public async Task<IActionResult> PostMessage([FromRoute] string sibyllaId, [FromRoute] string chatId, [FromBody] MessageDTO message)
    {
        return await _frontofficeService.AddMessageAndStreamAnswer(sibyllaId, chatId, message);
    }

    [HttpPost]
    [Route("sibylla/{sibyllaId}/chat/{chatId}/feedback")]
    [DynamicAuthorize("frontoffice")]
    public async Task<IActionResult> PostFeedback([FromRoute] string sibyllaId, [FromRoute] string chatId, [FromBody] FeedbackDTO feedback)
    {
        if (feedback.Text == null && feedback.Rating == null) return BadRequest();
        var chatDetail = await _chatDetailService.Get(Guid.Parse(chatId));
        if (chatDetail == null || chatDetail.sibyllaId != sibyllaId) return BadRequest();
        if (feedback.FactId != null && feedback.MessageId == null) return BadRequest();
        if (feedback.MessageId != null)
        {
            if (chatDetail.messagesIds == null) return BadRequest();
            if (!chatDetail.messagesIds.Any(m => m == feedback.MessageId)) return BadRequest();
            if (feedback.FactId != null)
            {
                var message = await _messageService.Get(Guid.Parse(feedback.MessageId));
                if (message == null) return StatusCode(StatusCodes.Status500InternalServerError);
                if (message.factIds == null && message.extraFactIds == null) return BadRequest();
                if (
                    message.factIds != null && !message.factIds.Any(f => f == feedback.FactId)
                    && message.extraFactIds != null && !message.extraFactIds.Any(f => f == feedback.FactId)
                    ) return BadRequest();
            }
        }

        var feedbackId = await _feedbackService.Add(new Feedback
        {
            text = feedback.Text,
            rating = feedback.Rating,
            sibyllaId = sibyllaId,
            chatId = chatId,
            messageId = feedback.MessageId,
            factId = feedback.FactId,
            timestamp = DateTime.Now
        });

        return Ok(new { id = feedbackId });
    }

    [HttpGet]
    [Route("reference/{id}")]
    [DynamicAuthorize("frontoffice")]
    public IActionResult GetReferenceById([FromRoute] string id)
    {
        throw new NotImplementedException();
    }

    //api di debug per fare le metriche, metodo sincrono
    [HttpPost]
    [Route("debug/answer")]
    [DynamicAuthorize("frontoffice")]
    public async Task<IActionResult> GetAnswerDebugAsync([FromBody] DebugRequestModel model)
    {
        if (model.OneShot)
            this.ClearSession();
        var (Sibylla, SibyllaRef) = await _frontofficeService.ConnectSibylla(HttpContext, _configuration["SibyllaConf"] ?? "Demo", HttpContext.Session.GetString("sibyllaRef"));
        HttpContext.Session.SetString("sibyllaRef", SibyllaRef);
        var answer = await Sibylla.Answer(model.Query);
        var prompt = Sibylla.Configuration.BaseSystemPrompt;
        var usedFacts = await _sibyllaManager.FindRelevantFacts(
                model.Query,
                factTypeFilter: Sibylla.Configuration.MemoryConfiguration.FactFilter,
                categoryFilter: Sibylla.Configuration.MemoryConfiguration.CategoryFilter,
                tagsFilter: Sibylla.Configuration.MemoryConfiguration.TagFilter,
                limit: Sibylla.Configuration.MemoryConfiguration.Limit,
                autoCutPercentage: Sibylla.Configuration.MemoryConfiguration.AutoCutPercentage
            );
        var factsList = usedFacts.Select(f => Models.BackOffice.Fact.FromOraculumFact(f)).ToList();
        return Ok(new { answer, prompt, factsList });
    }

    [HttpPost]
    [Route("sibylla/{sibyllaId}/chat-explain/{chatId}/message")]
    [DynamicAuthorize("frontoffice")]
    /* Known issue: Chatting on a chat previously used with the sibylla/{sibyllaId}/chat/{chatId}/message does not work due to issues with streamings. */
    public async Task<IActionResult> PostMessageExplain([FromRoute] string sibyllaId, [FromRoute] string chatId, [FromBody] MessageDTO message)
    {
        var (answer, prompt, userMessageId, assistantMessageId, usedFactsList, extraFactsList) = await _frontofficeService.AddMessageAndAnswerExplain(sibyllaId, chatId, message);

        return Ok(new { answer, prompt, userMessageId, assistantMessageId, usedFactsList, extraFactsList });
    }
}