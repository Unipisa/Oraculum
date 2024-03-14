using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;

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

    [HttpPost("factsFromText")]
    [Authorize(Policy = "BackOffice")]
    public async Task<IActionResult> ExtractFactsFromText([FromBody] dynamic jsonInput)
    {
        var taskId = await _dataIngestionService.ExtractFactsFromTextAsync(jsonInput);
        return Accepted(new { TaskId = taskId });
    }

    [HttpPost("factsFromWebPages")]
    [Authorize(Policy = "BackOffice")]
    public async Task<IActionResult> ExtractFactsFromWebPages([FromBody] dynamic jsonInput)
    {
        var taskId = await _dataIngestionService.ExtractFactsFromWebPagesAsync(jsonInput);
        return Accepted(new { TaskId = taskId });
    }

    [HttpPost("factsFromDocuments")]
    [Authorize(Policy = "BackOffice")]
    public async Task<IActionResult> ExtractFactsFromDocuments(IFormFile file)
    {
        var taskId = await _dataIngestionService.ExtractFactsFromDocumentsAsync(file);
        return Accepted(new { TaskId = taskId });
    }

    [HttpPost("factsFromAudioVideo")]
    [Authorize(Policy = "BackOffice")]
    public async Task<IActionResult> ExtractFactsFromAudioVideo(IFormFile file)
    {
        var taskId = await _dataIngestionService.ExtractFactsFromAudioVideoAsync(file);
        return Accepted(new { TaskId = taskId });
    }

    [HttpGet("checkStatus/{taskId}")]
    [Authorize(Policy = "BackOffice")]
    public IActionResult CheckStatus(string taskId)
    {
        var status = _dataIngestionService.CheckStatus(taskId);
        return Ok(new { TaskId = taskId, Status = status });
    }
}
