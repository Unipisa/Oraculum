using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using OraculumApi.Models.RestoreAndBackup;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1")]
public class BackupController : ControllerBase
{
    private readonly BackupService _backupService;

    public BackupController(BackupService backupService)
    {
        _backupService = backupService;
    }

    /// <summary>
    /// Create a new backup
    /// </summary>
    /// <remarks>
    /// Get id of the new backup
    /// </remarks>
    /// <response code="202">The backup is being created</response>
    /// <response code="400">Bad request. See response for details</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [DynamicAuthorize("sysadmin")]
    [ProducesResponseType(typeof(BackupIdDTO), 202)]
    public async Task<ActionResult<BackupIdDTO>> PrepareBackup()
    {
        var backupId = await _backupService.PrepareBackupAsync();
        return Accepted(new BackupIdDTO { BackupId = backupId });
    }

    /// <summary>
    /// List all backups
    /// </summary>
    /// <remarks>
    /// Show backups with id, creation time, state and progress (0 to 1)
    /// </remarks>
    /// <response code="200">List of backups</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [DynamicAuthorize("sysadmin")]
    public ActionResult<List<BackupStatusDTO>> GetBackupList()
    {
        var (backupIds, backupStatuses) = _backupService.GetBackupList();
        var backupList = backupIds.Zip(backupStatuses, (id, status) => new { id, status });
        return Ok(backupList);
    }

    /// <summary>
    /// Check status of a backup
    /// </summary>
    /// <remarks>
    /// Show backups with id, creation time, state and progress (0 to 1)
    /// </remarks>
    /// <response code="200">Backup status</response>
    /// <response code="404">Backup not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{backupId}/status")]
    [DynamicAuthorize("sysadmin")]
    public ActionResult<TaskStatus<BackupMetadataDTO>> CheckStatus([FromRoute] Guid backupId)
    {
        var status = _backupService.CheckStatus(backupId);
        return Ok(status);
    }

    /// <summary>
    /// Get a backup file
    /// </summary>
    /// <remarks>
    /// Download .orbak file of the backup
    /// </remarks>
    /// <response code="200">Backup file</response>
    /// <response code="404">Backup not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{backupId}")]
    [DynamicAuthorize("sysadmin")]
    public IActionResult ExportBackup([FromRoute] Guid backupId)
    {
        var streamReader = _backupService.ExportBackup(backupId);

        return File(streamReader.BaseStream, "application/octet-stream", $"{backupId}.orbak");
    }

    /// <summary>
    /// Delete a backup
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <response code="200">Backup deleted successfully</response>
    /// <response code="400">Bad request. See response for details</response>
    /// <response code="404">Backup not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{backupId}")]
    [DynamicAuthorize("sysadmin")]
    public ActionResult DeleteBackup([FromRoute] Guid backupId)
    {
        _backupService.DeleteBackup(backupId);

        return Ok();
    }

    /// <summary>
    /// Restore a backup
    /// </summary>
    /// <remarks>
    /// Upload an .orbak file and restore that backup
    /// </remarks>
    /// <response code="202">Request submitted successfully</response>
    /// <response code="400">Bad request. See response for details</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("Restore")]
    [DynamicAuthorize("sysadmin")]
    [ProducesResponseType(202)]
    public async Task<IActionResult> RestoreBackupAsync(IFormFile file)
    {

        if (file.Length <= 0)
            return BadRequest("File is empty");

        var filePath = Path.GetTempFileName();

        using (var stream = System.IO.File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }

        _backupService.RestoreBackupAsync(filePath);

        return Accepted();
    }

    /// <summary>
    /// Check last restore
    /// </summary>
    /// <remarks>
    /// Show last restore state and progress (0 to 1)
    /// </remarks>
    /// <response code="200">OK</response>
    /// <response code="204">No recent restore</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("Restore/Status")]
    [DynamicAuthorize("sysadmin")]
    public ActionResult<TaskStatus> CheckRestoreStatus()
    {
        var status = _backupService.CheckRestoreStatus();
        return Ok(status);
    }

}
