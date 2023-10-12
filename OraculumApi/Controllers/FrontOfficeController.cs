using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using OraculumApi.Attributes;

using Microsoft.AspNetCore.Authorization;
using OraculumApi.Models.FrontOffice;
using Oraculum;
using System.Diagnostics;
using System.Text.Json;
using System.Net.Http;
using System.Text;
using static OpenAI.ObjectModels.SharedModels.IOpenAiModels;

namespace OraculumApi.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1")]
[ApiVersion("2")]
[ApiController]
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

    //TODO: simply copied from the SibyllaSandbox controller, need to investigating on how it works
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> Index()
    {
        Sibylla sibylla = await ConnectSibylla();
        return View(sibylla);
    }

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
    [Route("/sibylla/{sibyllaId}/chat/{chatId}")]
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
    [Route("/sibylla")]
    public IActionResult GetAllSibyllae()
    {
        // Your logic here...
        throw new NotImplementedException();
    }

    [HttpGet]
    [Route("/sibylla/{sibyllaId}/chat")]
    public IActionResult GetChats([FromRoute] string sibyllaId)
    {
        // Your logic here...
        throw new NotImplementedException();
    }

    [HttpPost]
    [Route("/sibylla/{sibyllaId}/chat")]
    public IActionResult PostChat([FromRoute] string sibyllaId, [FromBody] Message message)
    {
        // Your logic here...
        throw new NotImplementedException();
    }

    [HttpGet]
    [Route("/sibylla/{sibyllaId}/chat/{chatId}")]
    public IActionResult GetChatById([FromRoute] string sibyllaId, [FromRoute] string chatId)
    {
        // Your logic here...
        throw new NotImplementedException();
    }

    [HttpPost]
    [Route("/sibylla/{sibyllaId}/chat/{chatId}/message")]
    public IActionResult PostMessage([FromRoute] string sibyllaId, [FromRoute] string chatId, [FromBody] Message message)
    {
        // Your logic here...
        throw new NotImplementedException();
    }

    [HttpPost]
    [Route("/sibylla/{sibyllaId}/chat/{chatId}/feedback")]
    public IActionResult PostFeedback([FromRoute] string sibyllaId, [FromRoute] string chatId, [FromBody] Feedback feedback)
    {
        // Your logic here...
        throw new NotImplementedException();
    }

    [HttpGet]
    [Route("/reference/{id}")]
    public IActionResult GetReferenceById([FromRoute] string id)
    {
        // Your logic here...
        throw new NotImplementedException();
    }
    [HttpPost]
    [Route("/answer/{question}")]
    public async Task<string> Answer([FromRoute][Required] string question)
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
    [Route("/getanswer/{answerId}")]
    public string GetAnswer([FromRoute][Required] string answerid)
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
}
