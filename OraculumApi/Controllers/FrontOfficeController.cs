using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using OraculumApi.Attributes;

using Microsoft.AspNetCore.Authorization;
using OraculumApi.Models;
using Oraculum;
using System.Diagnostics;
using System.Text.Json;
using System.Net.Http;
using System.Text;
using static OpenAI.ObjectModels.SharedModels.IOpenAiModels;

namespace OraculumApi.Controllers;

[ApiController]
[Route("[controller]")]
public class FrontOfficeController : ControllerBase
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
    public async Task<IActionResult> Index()
        {
            Sibylla sibylla = await ConnectSibylla();
            return View(sibylla);
        }

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
        public virtual IActionResult DeleteChatsChatId([FromRoute][Required]string chatId, [FromRoute][Required]string sibyllaId)
        { 
            //TODO: Uncomment the next line to return response 200 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
            return StatusCode(200);

            //TODO: Uncomment the next line to return response 404 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
            // return StatusCode(404);

            //TODO: Uncomment the next line to return response 500 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
            // return StatusCode(500);

            throw new NotImplementedException();
        }


    
}
