using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using OraculumApi.Attributes;
using OraculumApi.Models.BackOffice;
using Oraculum;
using System.Text;
using Asp.Versioning;
using System.Threading.Channels;
using OraculumApi.Models.FrontOffice;
using OraculumApi.Models;

namespace OraculumApi.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1")]
public class BackOfficeController : Controller
{
    private readonly ILogger<BackOfficeController> _logger;
    private readonly SibyllaManager _sibyllaManager;
    private readonly IConfiguration _configuration;
    private BaseService<Feedback> _feedbackService;
    private BaseService<SibyllaPersistentConfig> _sibyllaConfigService;

    public BackOfficeController(ILogger<BackOfficeController> logger, SibyllaManager sibyllaManager, IConfiguration configuration, BaseService<Feedback> feedbackService, BaseService<SibyllaPersistentConfig> sibyllaConfigService)
    {
        _logger = logger;
        _sibyllaManager = sibyllaManager;
        _configuration = configuration;
        _feedbackService = feedbackService;
        _sibyllaConfigService = sibyllaConfigService;
    }

    /// <summary>
    /// Create a new Sibylla configuration
    /// </summary>
    /// <remarks>Create a new Sibylla config</remarks>
    /// <param name="body">Sibylla configuration object to be added</param>
    /// <response code="201">Sibylla configuration added successfully</response>
    /// <response code="400">Invalid input</response>
    /// <response code="409">A Sibylla with the same name already exists</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [Route("sibylla-configs")]
    [ValidateModelState]
    [DynamicAuthorize("sysadmin")]
    [SwaggerOperation("AddSibyllaConfigDto")]
    public async Task<IActionResult> AddSibyllaConfigDtoAsync([FromBody] List<SibyllaConfigDto> body)
    {
        foreach (SibyllaConfigDto sibyllaConfig in body)
        {
            if (sibyllaConfig.Title == null)
            {
                // the title field is required because is being used as unique Id for teh configurations folder
                return StatusCode(400);
            }
            // save the single configuration 
            try
            {
                await Task.Run(() => _sibyllaManager.SaveSibylla(SibyllaConfigDto.FromSibyllaConfigDto(sibyllaConfig)));
            }
            catch (IOException)
            {
                // there was an error while saving the file on disk, maybe the file already exist
                return StatusCode(409, "Error: configuration with Id or title " + sibyllaConfig.Title + " already exist");
            }
        }
        return StatusCode(200);
    }

    /// <summary>
    /// Delete a fact by its ID
    /// </summary>
    /// <remarks>Delete a single Fact by ID</remarks>
    /// <param name="id">ID of the fact to delete</param>
    /// <response code="200">Fact deleted successfully</response>
    /// <response code="404">Fact not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete]
    [Route("facts/{id}")]
    [ValidateModelState]
    [DynamicAuthorize("backoffice")]
    [SwaggerOperation("DeleteFactById")]
    public async Task<IActionResult> DeleteFactById([FromRoute][Required] string id)
    {
        await _sibyllaManager.DeleteFactById(Guid.Parse(id));
        return Ok();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="factType"></param>
    /// <param name="category"></param>
    /// <param name="expired"></param>
    /// <response code="200">OK</response>
    [HttpDelete]
    [Route("facts")]
    [ValidateModelState]
    [DynamicAuthorize("backoffice")]
    [SwaggerOperation("DeleteFacts")]
    public virtual IActionResult DeleteFacts([FromQuery] string factType, [FromQuery] string category, [FromQuery] bool? expired)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Delete a Sibylla configuration by its ID
    /// </summary>
    /// <remarks>Delete a Sibylla config by ID</remarks>
    /// <param name="id">ID of the Sibylla configuration to delete</param>
    /// <response code="200">Configuration deleted successfully</response>
    /// <response code="404">Configuration not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete]
    [Route("sibylla-configs/{id}")]
    [ValidateModelState]
    [DynamicAuthorize("sysadmin")]
    [SwaggerOperation("DeleteSibyllaConfigDtoById")]
    public async Task<IActionResult> DeleteSibyllaConfigDtoById([FromRoute][Required] string id)
    {
        var result = await Task.Run(() => _sibyllaManager.DeleteSibyllaById(id));
        if (!result)
        {
            return StatusCode(404);
        }
        return StatusCode(200);
    }

    /// <summary>
    /// Find relevant facts based on provided criteria
    /// </summary>
    /// <remarks>Get relevant Facts using search criteria</remarks>
    /// <param name="body">Criteria to find relevant facts</param>
    /// <response code="200">List of relevant facts</response>
    /// <response code="400">Invalid input</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [Route("facts/query")]
    [ValidateModelState]
    [SwaggerOperation("FindRelevantFacts")]
    [DynamicAuthorize("backoffice")]
    [SwaggerResponse(statusCode: 200, type: typeof(List<Models.BackOffice.Fact>), description: "List of relevant facts")]
    public async Task<IActionResult> FindRelevantFacts([FromBody] SearchCriteria body)
    {
        var facts = await _sibyllaManager.FindRelevantFacts(body.Query, body.Distance, body.Limit, body.AutoCut, body.FactTypeFilter, body.CategoryFilter, body.AutoCutPercentage, body.TagsFilter);
        var factsList = facts.Select(f => Models.BackOffice.Fact.FromOraculumFact(f)).ToList();
        return Ok(factsList);
    }

    /// <summary>
    /// Retrieve all facts
    /// </summary>
    /// <remarks>Retrive a list of Facts using pagination (default max 10 elements)</remarks>
    /// <param name="perPage">Limit the number of facts returned.</param>
    /// <param name="page">Offset to start the facts list from.</param>
    /// <param name="sort">Attribute to sort the facts by.</param>
    /// <param name="order">Order of sorting (asc or desc).</param>
    /// <response code="200">A list of facts</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [Route("facts")]
    [ValidateModelState]
    [DynamicAuthorize("backoffice")]
    [SwaggerOperation("GetAllFacts")]
    [SwaggerResponse(statusCode: 200, type: typeof(List<Models.BackOffice.Fact>), description: "A list of facts")]
    public async Task<IActionResult> GetAllFacts([FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] string? sort, [FromQuery] string? order)
    {
        var facts = await _sibyllaManager.GetAllFacts(limit ?? 10, offset ?? 0, sort, order);
        var factsList = facts.Select(f => Models.BackOffice.Fact.FromOraculumFact(f)).ToList();
        return Ok(factsList);
    }

    /// <summary>
    /// Retrieve all Sibylla configurations
    /// </summary>
    /// <remarks>Retrive a list of Sibylla configs </remarks>
    /// <response code="200">List of Sibylla configurations</response>
    /// <response code="400">Bad Request</response>
    /// <response code="404">Not Found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [Route("sibylla-configs")]
    [ValidateModelState]
    [DynamicAuthorize("backoffice")]
    [SwaggerOperation("GetAllSibyllaConfigDtos")]
    [SwaggerResponse(statusCode: 200, type: typeof(List<SibyllaConfigDto>), description: "List of Sibylla configurations")]
    public async Task<IActionResult> GetAllSibyllaeConfigs()
    {
        var sibyllaeFromDB = await _sibyllaConfigService.List();
        var sibyllae = sibyllaeFromDB.Select(persistentConf =>
        {
            var sibyllaConf = SibyllaConf.FromJson(persistentConf.configJSON!);
            return new SibyllaConfigDto(persistentConf.name!)
            {
                Title = sibyllaConf?.Title,
                BaseAssistantPrompt = sibyllaConf?.BaseAssistantPrompt,
                BaseSystemPrompt = sibyllaConf?.BaseSystemPrompt,
                MaxTokens = sibyllaConf.MaxTokens,
                Model = sibyllaConf.Model,
                Temperature = sibyllaConf.Temperature,
                TopP = sibyllaConf.TopP,
                FrequencyPenalty = sibyllaConf.FrequencyPenalty,
                PresencePenalty = sibyllaConf.PresencePenalty,
                MemoryConfiguration = sibyllaConf.MemoryConfiguration,
                Hidden = sibyllaConf?.Hidden ?? false
            };
        }).ToList();

        if (_configuration.GetSection("DBSibyllaeConfigOnly").Get<bool>() != true)
        {
            sibyllae.AddRange(_sibyllaManager.GetSibyllaeDict().Select(conf => new SibyllaConfigDto(conf.Key)
            {
                Title = conf.Value.Title,
                BaseSystemPrompt = conf.Value.BaseSystemPrompt,
                BaseAssistantPrompt = conf.Value.BaseAssistantPrompt,
                MaxTokens = conf.Value.MaxTokens,
                Model = conf.Value.Model,
                Temperature = conf.Value.Temperature,
                TopP = conf.Value.TopP,
                FrequencyPenalty = conf.Value.FrequencyPenalty,
                PresencePenalty = conf.Value.PresencePenalty,
                MemoryConfiguration = conf.Value.MemoryConfiguration,
                Hidden = conf.Value.Hidden
            }));
        }

        return Ok(sibyllae);
    }

    /// <summary>
    /// Retrieve a fact by its ID
    /// </summary>
    /// <remarks>Get a single Fact by ID</remarks>
    /// <param name="id">ID of the fact to retrieve</param>
    /// <response code="200">Specific fact data</response>
    /// <response code="404">Fact not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [Route("facts/{id}")]
    [DynamicAuthorize("backoffice")]
    [SwaggerOperation("GetFactById")]
    [SwaggerResponse(statusCode: 200, type: typeof(Models.BackOffice.Fact), description: "Specific fact data")]
    public async Task<IActionResult> GetFactByIdAsync([FromRoute][Required] string id)
    {
        var fact = await _sibyllaManager.GetFactById(Guid.Parse(id));
        var factToReturn = Models.BackOffice.Fact.FromOraculumFact(fact);
        return StatusCode(200, factToReturn);
    }

    /// <summary>
    /// Retrieve a Sibylla configuration by its ID
    /// </summary>
    /// <remarks>Get a Sibylla config by ID</remarks>
    /// <param name="id">ID of the Sibylla configuration to retrieve</param>
    /// <response code="200">Specific Sibylla configuration data</response>
    /// <response code="404">Configuration not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [Route("sibylla-configs/{id}")]
    [ValidateModelState]
    [DynamicAuthorize("backoffice")]
    [SwaggerOperation("GetSibyllaConfigDtoById")]
    [SwaggerResponse(statusCode: 200, type: typeof(SibyllaConfigDto), description: "Specific Sibylla configuration data")]
    public async Task<IActionResult> GetSibyllaConfigDtoById([FromRoute][Required] string id)
    {
        var result = await Task.Run(() => _sibyllaManager.GetSibyllaConfById(id));
        if (result == null)
        {
            return StatusCode(404);
        }
        return StatusCode(200, SibyllaConfigDto.ToSibyllaConfigDto(result));
    }

    /// <summary>
    /// Add new Facts
    /// </summary>
    /// <remarks>Array of Fact objects that needs to be added</remarks>
    /// <param name="body"></param>
    /// <response code="200">OK</response>
    /// <response code="400">Bad Request</response>
    /// <response code="500">Internal Server Error</response>
    [HttpPost]
    [Route("facts")]
    [ValidateModelState]
    [DynamicAuthorize("backoffice")]
    [SwaggerOperation("PostFacts")]
    public async Task<IActionResult> PostFacts([FromBody] ICollection<Models.BackOffice.Fact> body)
    {
        await _sibyllaManager.AddFacts(body.Select(f => new Oraculum.Fact
        {
            id = f.Id,
            factType = f.FactType,
            category = f.Category,
            tags = f.Tags?.ToArray(),
            title = f.Title,
            content = f.Content,
            citation = f.Citation,
            reference = f.Reference,
            expiration = f.Expiration
        }).ToList());
        // return 200
        return Ok();
    }

    /// <summary>
    /// Edit Facts
    /// </summary>
    /// <remarks>Array of Fact objects that needs to be edited</remarks>
    /// <param name="body"></param>
    /// <response code="200">OK</response>
    /// <response code="400">Bad Request</response>
    /// <response code="404">Not Found</response>
    /// <response code="500">Internal Server Error</response>
    [HttpPut]
    [Route("facts")]
    [ValidateModelState]
    [DynamicAuthorize("backoffice")]
    [SwaggerOperation("PutFacts")]
    public async Task<IActionResult> PutFacts([FromBody] Models.BackOffice.Fact body)
    {
        await _sibyllaManager.UpdateFact(new Oraculum.Fact
        {
            id = body.Id,
            factType = body.FactType,
            category = body.Category,
            tags = body.Tags?.ToArray(),
            title = body.Title,
            content = body.Content,
            citation = body.Citation,
            reference = body.Reference,
            expiration = body.Expiration
        });

        return Ok();
    }

    /// <summary>
    /// Edit Sibylla configs
    /// </summary>
    /// <remarks>Edit a Sibylla configs</remarks>
    /// <param name="body">Sibylla configuration object to be edited</param>
    /// <response code="200">OK</response>
    /// <response code="400">Bad Request</response>
    /// <response code="404">Not Found</response>
    /// <response code="500">Internal Server Error</response>
    [HttpPut]
    [Route("sibylla-configs")]
    [ValidateModelState]
    [DynamicAuthorize("sysadmin")]
    [SwaggerOperation("PutSibyllaConfigDtos")]
    public async Task<IActionResult> PutSibyllaConfigDtos([FromBody] List<SibyllaConfigDto> body)
    {
        foreach (SibyllaConfigDto sibyllaConfig in body)
        {
            if (sibyllaConfig.Id == null)
            {
                // the Title or Id field is required because is being used as unique Id for teh configurations folder
                return StatusCode(400);
            }
            // update the single configuration 
            try
            {
                await Task.Run(() => _sibyllaManager.UpdateSibylla(SibyllaConfigDto.FromSibyllaConfigDto(sibyllaConfig)));
            }
            catch (Exception e)
            {
                // there was an error while saving the file on disk
                return StatusCode(400, e);
            }
        }
        return StatusCode(200);
    }

    // TODO:
    // temporary method waiting to have a real database with real IDs
    // Call this for obtaining a list of configurations with an Id based on the name of the json file from the configurations  folder
    private List<SibyllaConfigDto> getAllSibyllaeConfigs(List<SibyllaConf> sibyllaeConfs)
    {
        List<SibyllaConfigDto> result = new List<SibyllaConfigDto>();
        foreach (SibyllaConf s in sibyllaeConfs)
        {
            if (s.Title != null)
            {
                result.Add(SibyllaConfigDto.ToSibyllaConfigDto(s));
            }
        }
        return result;
    }

    private async Task WriteToChannel(Sibylla sibylla, string question, string answerid, ChannelWriter<string> writer)
    {
        await foreach (var fragment in sibylla.AnswerAsync(question))
        {
            await writer.WriteAsync(fragment);
        }
        writer.TryComplete();
    }

    // Method to add a new GenericObject
    [HttpPost]
    [Route("generic-objects")]
    public async Task<IActionResult> AddGenericObject([FromBody] GenericObject genericObject)
    {
        try
        {
            var id = await _sibyllaManager.AddGenericObjectAsync(genericObject);
            return Ok(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding GenericObject");
            return StatusCode(500, "Internal server error");
        }
    }

    // Method to get a GenericObject by ID
    [HttpGet]
    [Route("generic-objects/{id}")]
    public async Task<IActionResult> GetGenericObject([FromRoute] Guid id)
    {
        try
        {
            var genericObject = await _sibyllaManager.GetGenericObjectAsync(id);
            if (genericObject == null) return NotFound("GenericObject not found");

            return Ok(genericObject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving GenericObject");
            return StatusCode(500, "Internal server error");
        }
    }

    // Method to list all GenericObjects
    [HttpGet]
    [Route("generic-objects")]
    public async Task<IActionResult> ListGenericObjects([FromQuery] int limit = 1024, [FromQuery] int offset = 0)
    {
        try
        {
            var objects = await _sibyllaManager.ListGenericObjectsAsync(limit, offset);
            return Ok(objects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing GenericObjects");
            return StatusCode(500, "Internal server error");
        }
    }

    // Method to update a GenericObject
    [HttpPut]
    [Route("generic-objects")]
    public async Task<IActionResult> UpdateGenericObject([FromBody] GenericObject genericObject)
    {
        try
        {
            await _sibyllaManager.UpdateGenericObjectAsync(genericObject);
            return Ok("GenericObject updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating GenericObject");
            return StatusCode(500, "Internal server error");
        }
    }

    // Method to delete a GenericObject by ID
    [HttpDelete]
    [Route("generic-objects/{id}")]
    public async Task<IActionResult> DeleteGenericObject([FromRoute] Guid id)
    {
        try
        {
            var result = await _sibyllaManager.DeleteGenericObjectAsync(id);
            if (!result) return NotFound("GenericObject not found");

            return Ok("GenericObject deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting GenericObject");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet]
    [Route("feedback")]
    public async Task<IActionResult> ListFeedback([FromQuery] int limit = 1024, [FromQuery] int offset = 0)
    {
        var objects = await _feedbackService.List(limit, offset);
        return Ok(objects.Select(f => f.toDTO()).ToList());
    }
}
