using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using OraculumApi.Attributes;
using OraculumApi.Models.FrontOffice;
using Oraculum;
using System.Text;
using System.Threading.Channels;
using Microsoft.OpenApi.Any;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;

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

    public FrontOfficeController(ILogger<FrontOfficeController> logger, SibyllaManager sibyllaManager, IConfiguration configuration)
    {
        _logger = logger;
        _sibyllaManager = sibyllaManager;
        _configuration = configuration;
    }

    //only to temporarily make the apis work
    //TODO: simply copied from the SibyllaSandbox controller, need to investigating on how it works
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> Index()
    {
        Sibylla sibylla = await ConnectSibylla();
        return View(sibylla);
    }

    //only to temporarily make the apis work
    //TODO: simply copied from the SibyllaSandbox controller, need to investigating on how it works
    [ApiExplorerSettings(IgnoreApi = true)]
    private async Task<Sibylla> ConnectSibylla()
    {
        var sibyllaName = _configuration["SibyllaConf"] ?? "Demo";
        var sibyllaKey = HttpContext.Session.GetString("sibyllaRef");
        if (sibyllaKey == null)
        {
            var (id, _) = await _sibyllaManager.AddSibylla(sibyllaName, expiration: DateTime.Now.AddMinutes(60));
            HttpContext.Session.SetString("sibyllaRef", id.ToString());
            sibyllaKey = id.ToString();
        }
        var sibylla = _sibyllaManager.GetSibylla(sibyllaName, Guid.Parse(sibyllaKey));
        return sibylla;
    }

    /// <summary>
    /// Delete a chat by id
    /// </summary>
    /// <param name="chatId"></param>
    /// <param name="sibyllaId"></param>
    /// <response code="200">OK</response>
    /// <response code="404">Not Found</response>
    /// <response code="500">Internal Server Error</response>
    [HttpDelete]
    [Route("sibylla/{sibyllaId}/chat/{chatId}")]
    [ValidateModelState]
    [Authorize(Policy = "FrontOffice")]
    [SwaggerOperation("DeleteChatsChatId")]
    public virtual IActionResult DeleteChatsChatId([FromRoute][Required] string chatId, [FromRoute][Required] string sibyllaId)
    {
        throw new NotImplementedException();
    }

    [HttpGet]
    [Route("sibylla")]
    [Authorize(Policy = "FrontOffice")]
    public IActionResult GetAllSibyllae()
    {
        throw new NotImplementedException();
    }

    [HttpGet]
    [Route("sibylla/{sibyllaId}/chat")]
    [Authorize(Policy = "FrontOffice")]
    public IActionResult GetChats([FromRoute] string sibyllaId)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Route("sibylla/{sibyllaId}/chat")]
    [Authorize(Policy = "FrontOffice")]
    public IActionResult PostChat([FromRoute] string sibyllaId, [FromBody] Message message)
    {
        throw new NotImplementedException();
    }

    [HttpGet]
    [Route("sibylla/{sibyllaId}/chat/{chatId}")]
    [Authorize(Policy = "FrontOffice")]
    public IActionResult GetChatById([FromRoute] string sibyllaId, [FromRoute] string chatId)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Route("sibylla/{sibyllaId}/chat/{chatId}/message")]
    [Authorize(Policy = "FrontOffice")]
    public IActionResult PostMessage([FromRoute] string sibyllaId, [FromRoute] string chatId, [FromBody] Message message)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Route("sibylla/{sibyllaId}/chat/{chatId}/feedback")]
    [Authorize(Policy = "FrontOffice")]
    public IActionResult PostFeedback([FromRoute] string sibyllaId, [FromRoute] string chatId, [FromBody] Feedback feedback)
    {
        throw new NotImplementedException();
    }

    [HttpGet]
    [Route("reference/{id}")]
    [Authorize(Policy = "FrontOffice")]
    public IActionResult GetReferenceById([FromRoute] string id)
    {
        throw new NotImplementedException();
    }


    [HttpPost]
    [Route("answerStream/{question}")]
    [Authorize(Policy = "FrontOffice")]
    public async Task<IActionResult> AnswerStream([FromRoute][Required] string question)
    {
        var Sibylla = await ConnectSibylla();
        var answerid = Guid.NewGuid().ToString();

        var channel = Channel.CreateUnbounded<string>();

        _ = WriteToChannel(Sibylla, question, answerid, channel.Writer);

        // Stream the response as Server-Sent Events
        return new PushStreamResult(
            async (stream, _, cancellationToken) =>
            {
                var writer = new StreamWriter(stream);
                await foreach (var chunk in channel.Reader.ReadAllAsync(cancellationToken))
                {
                    if (!string.IsNullOrEmpty(chunk))
                    {
                        await writer.WriteAsync($"data: {chunk}\n\n");
                        await writer.FlushAsync();
                    }
                }
            }, "text/event-stream");
    }

    [HttpPost]
    [Route("answer/{question}")]
    [Authorize(Policy = "FrontOffice")]
    public async Task<string> Answer(string question)
    {
        var Sibylla = await ConnectSibylla();
        var answerid = Guid.NewGuid().ToString();
        _sibyllaManager.Response.Add(answerid, new List<string>());
        var t = Task.Run(() =>
        {
            var ena = Sibylla.AnswerAsync(question);
            var en = ena.GetAsyncEnumerator();
            while (true)
            {
                var j = en.MoveNextAsync();
                j.AsTask().Wait();
                if (!j.Result)
                    break;
                lock (_sibyllaManager.Response)
                {
                    _sibyllaManager.Response[answerid].Add(en.Current);
                }
            }
            lock (_sibyllaManager.Completed)
            {
                _sibyllaManager.Completed.Add(answerid);
            }
        }
        );
        return answerid;
    }

    [HttpGet]
    [Route("getanswer/{answerid}")]
    [Authorize(Policy = "FrontOffice")]
    public string GetAnswer(string answerid)
    {
        lock (_sibyllaManager.Response)
        {
            if (!_sibyllaManager.Response.ContainsKey(answerid))
            {
                HttpContext.Response.StatusCode = 204;
                return "";
            }
            var r = _sibyllaManager.Response[answerid];
            if (r.Count == 0 && _sibyllaManager.Completed.Contains(answerid))
            {
                _sibyllaManager.Response.Remove(answerid);
                _sibyllaManager.Completed.Remove(answerid);
                HttpContext.Response.StatusCode = 204;
                return "";
            }
            var ret = new StringBuilder();
            foreach (var s in r)
            {
                ret.Append(s);
            }
            r.Clear();
            return ret.ToString();
        }
    }

    //api di debug per fare le metriche, metodo sincrono
    [HttpGet]
    [Route("getanswer/debug/{query}")]
    [Authorize(Policy = "FrontOffice")]
    public async Task<IActionResult> GetAnswerDebugAsync(string query, int limit = 10)
    {
        var Sibylla = await ConnectSibylla();
        var answer = await Sibylla.Answer(query);
        var prompt = Sibylla.Configuration.BaseSystemPrompt;
        var facts = await _sibyllaManager.FindRelevantFacts(query, null, limit);
        var factsList = facts.Select(f => Models.BackOffice.Fact.FromOraculumFact(f)).ToList();
        return Ok(new { answer, prompt, factsList });
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

public class PushStreamResult : IActionResult
{
    private readonly Func<Stream, Action<Exception>, CancellationToken, Task> _onStreamAvailable;
    private readonly string _contentType;

    public PushStreamResult(Func<Stream, Action<Exception>, CancellationToken, Task> onStreamAvailable, string contentType = null)
    {
        _onStreamAvailable = onStreamAvailable ?? throw new ArgumentNullException(nameof(onStreamAvailable));
        _contentType = contentType ?? "text/event-stream";
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        var response = context.HttpContext.Response;
        response.ContentType = _contentType;
        response.Headers["Cache-Control"] = "no-store, no-cache";
        response.Headers["Connection"] = "keep-alive";

        await _onStreamAvailable(response.Body, ex =>
        {
            context.HttpContext.Abort();
        }, context.HttpContext.RequestAborted);
    }
}

