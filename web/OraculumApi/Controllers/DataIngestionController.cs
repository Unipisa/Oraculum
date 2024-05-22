using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using Oraculum;
using OraculumApi.Models;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1")]
public class DataIngestionController : ControllerBase
{
    private readonly IDataIngestionService _dataIngestionService;

    public DataIngestionController(IDataIngestionService dataIngestionService)
    {
        _dataIngestionService = dataIngestionService;
    }

    /// <summary>
    /// Creates new facts from a text
    /// </summary>
    /// <remarks>
    /// Submit a task request and get its task id.
    /// </remarks>
    /// <response code="200">Task created succesfully</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("factsFromText")]
    [DynamicAuthorize("BackOffice")]
    public async Task<IActionResult> ExtractFactsFromText([FromBody] List<DataIngestTextDTO> jsonInput, string? category = null)
    {
        var taskId = await _dataIngestionService.ExtractFactsFromTextAsync(jsonInput, category);
        return Accepted(new { TaskId = taskId });
    }

    /// <summary>
    /// Creates new facts from a webpage
    /// </summary>
    /// <remarks>
    ///  Submit a task request and get its task id.
    /// </remarks>
    /// <response code="202">Task created successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("factsFromWebPages")]
    [DynamicAuthorize("BackOffice")]
    [SwaggerResponse(statusCode: 200, type: typeof(string))]
    public async Task<IActionResult> ExtractFactsFromWebPages([FromBody] DataIngestWebPagesDTO jsonInput, string? category = null)
    {
        var taskId = await _dataIngestionService.ExtractFactsFromWebPagesAsync(jsonInput, category);
        return Accepted(new { TaskId = taskId });
    }

    /// <summary>
    /// Creates new facts from a document
    /// </summary>
    /// <remarks>
    /// Submit a task request and get its task id.
    /// Supports .pdf and .docx
    /// </remarks>
    /// <response code="202">Task created succesfully</response>
    /// <response code="400">Bad request</response>
    /// <response code="415">Unsupported media type</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("factsFromDocuments")]
    [DynamicAuthorize("BackOffice")]
    [SwaggerResponse(statusCode: 200, type: typeof(string))]
    public async Task<IActionResult> ExtractFactsFromDocuments(IFormFile file, string category)
    {
        var taskId = await _dataIngestionService.ExtractFactsFromDocumentsAsync(file, category);
        return Accepted(new { TaskId = taskId });
    }

    /// <summary>
    /// Creates new facts from audio or video
    /// </summary>
    /// <remarks>
    /// Submit a task request and get its task id.
    /// Supports .mp3 and .mp4
    /// </remarks>
    /// <response code="202">Task created succesfully</response>
    /// <response code="400">Bad request</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("factsFromAudioVideo")]
    [DynamicAuthorize("BackOffice")]
    [SwaggerResponse(statusCode: 200, type: typeof(string))]
    public async Task<IActionResult> ExtractFactsFromAudioVideo(IFormFile file, string category)
    {
        var taskId = await _dataIngestionService.ExtractFactsFromAudioVideoAsync(file, category);
        return Accepted(new { TaskId = taskId });
    }


    public class TaskStatusResponse
    {
        public string TaskId { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// Check task status by id
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <response code="200">Status of the task</response>
    /// <response code="400">Bad request</response>
    /// <response code="404">Not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("checkStatus/{taskId}")]
    [DynamicAuthorize("BackOffice")]
    [SwaggerResponse(statusCode: 200, type: typeof(TaskStatusResponse))]

    public IActionResult CheckStatus(string taskId)
    {
        var status = _dataIngestionService.CheckStatus(taskId);
        return Ok(new { TaskId = taskId, Status = status });
    }
}
