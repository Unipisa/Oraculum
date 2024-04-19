using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using OraculumApi.Models;
using Swashbuckle.AspNetCore.Annotations;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1")]
public class EvaluateController : ControllerBase
{
    private readonly EvaluateService _EvaluateService;

    public EvaluateController(EvaluateService EvaluateService)
    {
        _EvaluateService = EvaluateService;
    }

    /// <summary>
    /// Evaluate a testset
    /// </summary>
    /// <remarks>
    /// Submit a task request and get its task id.
    /// Question, Answers and Contexts are required.
    /// Ground Truths are recommended to obtain all the available metrics.
    /// </remarks>
    /// <response code="200">Task submitted</response>
    /// <response code="400">Bad request</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("sibylla/{sibyllaId}/Evaluate")]
    [DynamicAuthorize("BackOffice")]
    [SwaggerResponse(statusCode: 200, type: typeof(EvaluateResponseDTO))]
    public async Task<IActionResult> Evaluate([FromBody] List<EvaluateResponseDTO> jsonInput, [FromRoute] string sibyllaId)
    {
        var taskId = await _EvaluateService.EvaluateAsync(jsonInput, sibyllaId);
        return Ok(taskId);
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
    [HttpGet("TaskStatus/{taskId}")]
    [DynamicAuthorize("BackOffice")]
    [SwaggerResponse(statusCode: 200, type: typeof(object))]
    public IActionResult TaskStatus([FromRoute] string taskId)
    {
        var status = _EvaluateService.CheckStatus(taskId);

        if (status == null)
        {
            return NotFound();
        }

        return Ok(status);
    }

    /// <summary>
    /// Get Report by id
    /// </summary>
    /// <remarks>
    /// Submit a resultId obtained from the TaskStatus 
    /// to retrieve the evaluation report in Excel format.
    /// </remarks>
    /// <response code="200">Status of the task</response>
    /// <response code="400">Bad request</response>
    /// <response code="404">Not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("report")]
    public async Task<IActionResult> GetReport(string evaluateId)
    {
        string propertyName = "evaluateId";
        int limit = 5024;
        int offset = 0;

        var workbook = await _EvaluateService.GetReport(evaluateId, propertyName, limit, offset);

        using (var stream = new MemoryStream())
        {
            workbook.SaveAs(stream);
            var content = stream.ToArray();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "report.xlsx");
        }
    }
}
