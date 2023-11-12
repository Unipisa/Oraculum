using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using OraculumApi.Attributes;
using OraculumApi.Models.BackOffice;
using Oraculum;
using System.Text;
using System.Runtime.CompilerServices;
using Microsoft.OpenApi.Any;
using Microsoft.AspNetCore.Http.HttpResults;
using Asp.Versioning;

namespace OraculumApi.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1")]
[ApiVersion("2")]
public class BackOfficeController : Controller
{

    private readonly ILogger<BackOfficeController> _logger;
    private readonly SibyllaManager _sibyllaManager;
    private readonly IConfiguration _configuration;

    public BackOfficeController(ILogger<BackOfficeController> logger, SibyllaManager sibyllaManager, IConfiguration configuration)
    {
        _logger = logger;
        _sibyllaManager = sibyllaManager;
        _configuration = configuration;
    }

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
            var (id, _) = await _sibyllaManager.AddSibylla(sibyllaName, expiration: DateTime.Now.AddMinutes(60));
            HttpContext.Session.SetString("sibyllaRef", id.ToString());
            sibyllaKey = id.ToString();
        }
        var sibylla = _sibyllaManager.GetSibylla(sibyllaName, Guid.Parse(sibyllaKey));
        return sibylla;
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
    [SwaggerResponse(statusCode: 200, type: typeof(List<Models.BackOffice.Fact>), description: "List of relevant facts")]
    public async Task<IActionResult> FindRelevantFacts([FromBody] SearchCriteria body)
    {
        var facts = await _sibyllaManager.FindRelevantFacts(body.Query, body.Distance, body.Limit, body.AutoCut, body.FactTypeFilter, body.CategoryFilter, body.TagsFilter);
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
    [SwaggerOperation("GetAllSibyllaConfigDtos")]
    [SwaggerResponse(statusCode: 200, type: typeof(List<SibyllaConfigDto>), description: "List of Sibylla configurations")]
    public async Task<IActionResult> GetAllSibyllaeConfigs()
    {
        List<SibyllaConf> sibyllaeConfs = await Task.Run(() => _sibyllaManager.GetSibyllae());
        if (sibyllaeConfs == null)
            return StatusCode(500);
        var result = getAllSibyllaeConfigs(sibyllaeConfs);
        if (result == null)
        {
            return StatusCode(500);
        }
        return StatusCode(200, result);
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
    [SwaggerOperation("GetSibyllaConfigDtoById")]
    [SwaggerResponse(statusCode: 200, type: typeof(SibyllaConfigDto), description: "Specific Sibylla configuration data")]
    public async Task<IActionResult> GetSibyllaConfigDtoById([FromRoute][Required] string id)
    {
        var result = await Task.Run(() => _sibyllaManager.GetSibyllaById(id));
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
    [SwaggerOperation("PostFacts")]
    public async Task<IActionResult> PostFacts([FromBody] ICollection<Models.BackOffice.Fact> body)
    {
        await _sibyllaManager.AddFacts(body.Select(f => new Oraculum.Fact
        {
            id = f.Id,
            factType = f.FactType,
            category = f.Category,
            tags = f.Tags.ToArray(),
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
    [SwaggerOperation("PutFacts")]
    public virtual IActionResult PutFacts([FromBody] List<Models.BackOffice.Fact> body)
    {
        throw new NotImplementedException();
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
}
