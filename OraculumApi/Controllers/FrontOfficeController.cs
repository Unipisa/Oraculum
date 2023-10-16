using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using OraculumApi.Attributes;
using OraculumApi.Models.FrontOffice;
using Oraculum;
using System.Text;
using System.Threading.Channels;

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
            // It would be nice to align the expiration of the Sibylla with the expiration of the session.
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
    [SwaggerOperation("DeleteChatsChatId")]
    public virtual IActionResult DeleteChatsChatId([FromRoute][Required] string chatId, [FromRoute][Required] string sibyllaId)
    {
        //TODO: Uncomment the next line to return response 200 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        return StatusCode(200);

        //TODO: Uncomment the next line to return response 404 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(404);

        //TODO: Uncomment the next line to return response 500 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(500);

        throw new NotImplementedException();
    }

    [HttpGet]
    [Route("sibylla")]
    public IActionResult GetAllSibyllae()
    {
        // Your logic here...
        throw new NotImplementedException();
    }

    [HttpGet]
    [Route("sibylla/{sibyllaId}/chat")]
    public IActionResult GetChats([FromRoute] string sibyllaId)
    {
        // Your logic here...
        throw new NotImplementedException();
    }

    [HttpPost]
    [Route("sibylla/{sibyllaId}/chat")]
    public IActionResult PostChat([FromRoute] string sibyllaId, [FromBody] Message message)
    {
        // Your logic here...
        throw new NotImplementedException();
    }

    [HttpGet]
    [Route("sibylla/{sibyllaId}/chat/{chatId}")]
    public IActionResult GetChatById([FromRoute] string sibyllaId, [FromRoute] string chatId)
    {
        // Your logic here...
        throw new NotImplementedException();
    }

    [HttpPost]
    [Route("sibylla/{sibyllaId}/chat/{chatId}/message")]
    public IActionResult PostMessage([FromRoute] string sibyllaId, [FromRoute] string chatId, [FromBody] Message message)
    {
        // Your logic here...
        throw new NotImplementedException();
    }

    [HttpPost]
    [Route("sibylla/{sibyllaId}/chat/{chatId}/feedback")]
    public IActionResult PostFeedback([FromRoute] string sibyllaId, [FromRoute] string chatId, [FromBody] Feedback feedback)
    {
        // Your logic here...
        throw new NotImplementedException();
    }

    [HttpGet]
    [Route("reference/{id}")]
    public IActionResult GetReferenceById([FromRoute] string id)
    {
        // Your logic here...
        throw new NotImplementedException();
    }

    [HttpPost]
    [Route("answer/{question}")]
    public async Task<IActionResult> Answer([FromRoute][Required] string question)
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

