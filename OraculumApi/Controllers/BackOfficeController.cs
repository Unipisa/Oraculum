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
            // It would be nice to align the expiration of the Sibylla with the expiration of the session.
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
    [SwaggerOperation("AddSibyllaConfig")]
    public virtual IActionResult AddSibyllaConfig([FromBody] List<SibyllaConfig> body)
    {
        //TODO: Uncomment the next line to return response 201 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(201);

        //TODO: Uncomment the next line to return response 400 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(400);

        //TODO: Uncomment the next line to return response 409 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(409);

        //TODO: Uncomment the next line to return response 500 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(500);

        throw new NotImplementedException();
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
    public virtual IActionResult DeleteFactById([FromRoute][Required] string id)
    {
        //TODO: Uncomment the next line to return response 200 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(200);

        //TODO: Uncomment the next line to return response 404 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(404);

        //TODO: Uncomment the next line to return response 500 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(500);

        throw new NotImplementedException();
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
        //TODO: Uncomment the next line to return response 200 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(200);

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
    [SwaggerOperation("DeleteSibyllaConfigById")]
    public virtual IActionResult DeleteSibyllaConfigById([FromRoute][Required] string id)
    {
        //TODO: Uncomment the next line to return response 200 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(200);

        //TODO: Uncomment the next line to return response 404 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(404);

        //TODO: Uncomment the next line to return response 500 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(500);

        throw new NotImplementedException();
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
    [SwaggerResponse(statusCode: 200, type: typeof(List<Oraculum.Fact>), description: "List of relevant facts")]
    public virtual IActionResult FindRelevantFacts([FromBody] SearchCriteria body)
    {
        //TODO: Uncomment the next line to return response 200 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(200, default(List<Fact>));

        //TODO: Uncomment the next line to return response 400 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(400);

        //TODO: Uncomment the next line to return response 500 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(500);
        string? exampleJson = null;
        exampleJson = "[ {\n  \"reference\" : \"reference\",\n  \"citation\" : \"citation\",\n  \"factType\" : \"factType\",\n  \"expiration\" : \"2000-01-23T04:56:07.000+00:00\",\n  \"id\" : \"id\",\n  \"category\" : \"category\",\n  \"title\" : \"title\",\n  \"content\" : \"content\",\n  \"tags\" : [ \"tags\", \"tags\" ]\n}, {\n  \"reference\" : \"reference\",\n  \"citation\" : \"citation\",\n  \"factType\" : \"factType\",\n  \"expiration\" : \"2000-01-23T04:56:07.000+00:00\",\n  \"id\" : \"id\",\n  \"category\" : \"category\",\n  \"title\" : \"title\",\n  \"content\" : \"content\",\n  \"tags\" : [ \"tags\", \"tags\" ]\n} ]";

        var example = exampleJson != null
        ? JsonConvert.DeserializeObject<List<Oraculum.Fact>>(exampleJson)
        : default(List<Oraculum.Fact>);            //TODO: Change the data returned
        return new ObjectResult(example);
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
    [SwaggerResponse(statusCode: 200, type: typeof(List<Oraculum.Fact>), description: "A list of facts")]
    public virtual IActionResult GetAllFacts([FromQuery] int? perPage, [FromQuery] int? page, [FromQuery] string sort, [FromQuery] string order)
    {
        //TODO: Uncomment the next line to return response 200 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(200, default(List<Fact>));

        //TODO: Uncomment the next line to return response 500 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(500);
        string? exampleJson = null;
        exampleJson = "[ {\n  \"reference\" : \"reference\",\n  \"citation\" : \"citation\",\n  \"factType\" : \"factType\",\n  \"expiration\" : \"2000-01-23T04:56:07.000+00:00\",\n  \"id\" : \"id\",\n  \"category\" : \"category\",\n  \"title\" : \"title\",\n  \"content\" : \"content\",\n  \"tags\" : [ \"tags\", \"tags\" ]\n}, {\n  \"reference\" : \"reference\",\n  \"citation\" : \"citation\",\n  \"factType\" : \"factType\",\n  \"expiration\" : \"2000-01-23T04:56:07.000+00:00\",\n  \"id\" : \"id\",\n  \"category\" : \"category\",\n  \"title\" : \"title\",\n  \"content\" : \"content\",\n  \"tags\" : [ \"tags\", \"tags\" ]\n} ]";

        var example = exampleJson != null
        ? JsonConvert.DeserializeObject<List<Oraculum.Fact>>(exampleJson)
        : default(List<Oraculum.Fact>);            //TODO: Change the data returned
        return new ObjectResult(example);
    }

    /// <summary>
    /// Retrieve all Sibylla configurations
    /// </summary>
    /// <remarks>Retrive a list of Sibylla configs using pagination (default max 10 elements)</remarks>
    /// <param name="perPage">Limit the number of facts returned.</param>
    /// <param name="page">Offset to start the facts list from.</param>
    /// <param name="sort">Attribute to sort the facts by.</param>
    /// <param name="order">Order of sorting (asc or desc).</param>
    /// <response code="200">List of Sibylla configurations</response>
    /// <response code="400">Bad Request</response>
    /// <response code="404">Not Found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [Route("sibylla-configs")]
    [ValidateModelState]
    [SwaggerOperation("GetAllSibyllaConfigs")]
    [SwaggerResponse(statusCode: 200, type: typeof(List<SibyllaConfig>), description: "List of Sibylla configurations")]
    public virtual IActionResult GetAllSibyllaConfigs([FromQuery] int? perPage, [FromQuery] int? page, [FromQuery] string sort, [FromQuery] string order)
    {
        //TODO: Uncomment the next line to return response 200 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(200, default(List<SibyllaConfig>));

        //TODO: Uncomment the next line to return response 400 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(400);

        //TODO: Uncomment the next line to return response 404 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(404);

        //TODO: Uncomment the next line to return response 500 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(500);
        string? exampleJson = null;
        exampleJson = "[ {\n  \"systemPrompt\" : \"systemPrompt\",\n  \"presencePenalty\" : 5.637377,\n  \"assistantPrompt\" : \"assistantPrompt\",\n  \"file\" : \"file\",\n  \"maxTokens\" : 0,\n  \"temperature\" : 6.0274563,\n  \"model\" : \"gpt-3.5-turbo\",\n  \"id\" : \"id\",\n  \"topP\" : 1.4658129,\n  \"frequencyPenalty\" : 5.962134\n}, {\n  \"systemPrompt\" : \"systemPrompt\",\n  \"presencePenalty\" : 5.637377,\n  \"assistantPrompt\" : \"assistantPrompt\",\n  \"file\" : \"file\",\n  \"maxTokens\" : 0,\n  \"temperature\" : 6.0274563,\n  \"model\" : \"gpt-3.5-turbo\",\n  \"id\" : \"id\",\n  \"topP\" : 1.4658129,\n  \"frequencyPenalty\" : 5.962134\n} ]";

        var example = exampleJson != null
        ? JsonConvert.DeserializeObject<List<SibyllaConfig>>(exampleJson)
        : default(List<SibyllaConfig>);            //TODO: Change the data returned
        return new ObjectResult(example);
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
    [SwaggerResponse(statusCode: 200, type: typeof(Oraculum.Fact), description: "Specific fact data")]
    public async Task<IActionResult> GetFactByIdAsync([FromRoute][Required] string id)
    {
        // get Fact from Oraculum and return it
        var fact = await _sibyllaManager.GetFactById(Guid.Parse(id));
        // check if fact is null
        if (fact == null)
        {
            // return 404
            return NotFound();
        }
        return Ok(fact);
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
    [SwaggerOperation("GetSibyllaConfigById")]
    [SwaggerResponse(statusCode: 200, type: typeof(SibyllaConfig), description: "Specific Sibylla configuration data")]
    public virtual IActionResult GetSibyllaConfigById([FromRoute][Required] string id)
    {
        //TODO: Uncomment the next line to return response 200 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(200, default(SibyllaConfig));

        //TODO: Uncomment the next line to return response 404 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(404);

        //TODO: Uncomment the next line to return response 500 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(500);
        string? exampleJson = null;
        exampleJson = "{\n  \"systemPrompt\" : \"systemPrompt\",\n  \"presencePenalty\" : 5.637377,\n  \"assistantPrompt\" : \"assistantPrompt\",\n  \"file\" : \"file\",\n  \"maxTokens\" : 0,\n  \"temperature\" : 6.0274563,\n  \"model\" : \"gpt-3.5-turbo\",\n  \"id\" : \"id\",\n  \"topP\" : 1.4658129,\n  \"frequencyPenalty\" : 5.962134\n}";

        var example = exampleJson != null
        ? JsonConvert.DeserializeObject<SibyllaConfig>(exampleJson)
        : default(SibyllaConfig);            //TODO: Change the data returned
        return new ObjectResult(example);
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
    public virtual IActionResult PostFacts([FromBody] List<Oraculum.Fact> body)
    {
        //TODO: Uncomment the next line to return response 200 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(200);

        //TODO: Uncomment the next line to return response 400 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(400);

        //TODO: Uncomment the next line to return response 500 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(500);

        throw new NotImplementedException();
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
    public virtual IActionResult PutFacts([FromBody] List<Oraculum.Fact> body)
    {
        //TODO: Uncomment the next line to return response 200 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(200);

        //TODO: Uncomment the next line to return response 400 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(400);

        //TODO: Uncomment the next line to return response 404 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(404);

        //TODO: Uncomment the next line to return response 500 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(500);

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
    [SwaggerOperation("PutSibyllaConfigs")]
    public virtual IActionResult PutSibyllaConfigs([FromBody] List<SibyllaConfig> body)
    {
        //TODO: Uncomment the next line to return response 200 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(200);

        //TODO: Uncomment the next line to return response 400 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(400);

        //TODO: Uncomment the next line to return response 404 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(404);

        //TODO: Uncomment the next line to return response 500 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
        // return StatusCode(500);

        throw new NotImplementedException();
    }
}
