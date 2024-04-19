using System.Collections.Concurrent;
using System.Collections.Immutable;
using Oraculum;
using OraculumApi.Models.RestoreAndBackup;


public class BackupService
{
    private readonly SibyllaManager _sibyllaManager;
    private static ConcurrentDictionary<Guid, TaskStatus<BackupMetadataDTO>> _backupTasksStatus = new();
    private static TaskStatus? _restoreTaskStatus;
    private readonly object _lock = new();

    public BackupService(SibyllaManager sibyllaManager)
    {
        _sibyllaManager = sibyllaManager;
    }

    public Task<Guid> PrepareBackupAsync()
    {
        var backupId = Guid.NewGuid();

        lock (_lock)
        {
            if (_restoreTaskStatus != null && _restoreTaskStatus.State == TaskState.Processing)
                throw new HttpResponseException(400, "Cannot start backup while a restore task is in progress.");

            _backupTasksStatus.TryAdd(backupId, new() { State = TaskState.Processing, Metadata = new() { CreationTime = DateTime.Now } });
        }

        Task.Run(async () =>
        {
            try
            {
                Directory.CreateDirectory("/backup");
                var path = Path.Combine("/backup/", backupId.ToString() + ".orbak");
                await _sibyllaManager.BackupFacts(path, (current, total) => UpdateBackupProgress(backupId, current, total));
                _backupTasksStatus[backupId].State = TaskState.Completed;
                _backupTasksStatus[backupId].Progress = 1.0f;
                _backupTasksStatus[backupId].ResultId = backupId;
            }
            catch (Exception)
            {
                _backupTasksStatus[backupId].State = TaskState.Failed;
            }
        });

        return Task.FromResult(backupId);
    }

    public TaskStatus<BackupMetadataDTO>? CheckStatus(Guid backupId)
    {
        if (!_backupTasksStatus.ContainsKey(backupId))
            throw new HttpResponseException(404, "Backup not found");

        _backupTasksStatus.TryGetValue(backupId, out TaskStatus<BackupMetadataDTO>? status);
        return status;
    }

    public StreamReader ExportBackup(Guid backupId)
    {
        if (_backupTasksStatus.TryGetValue(backupId, out TaskStatus<BackupMetadataDTO>? status))
        {
            if (status.State != TaskState.Completed)
                throw new HttpResponseException(400, "Backup not ready or failed");

            var path = Path.Combine("/backup/", backupId.ToString() + ".orbak");
            return new StreamReader(path);
        }
        else
        {
            throw new HttpResponseException(404, "Backup not found");
        }
    }

    public (Guid[], TaskStatus<BackupMetadataDTO>[]) GetBackupList()
    {
        var backupIds = _backupTasksStatus.Keys.ToArray();
        var backupStatuses = _backupTasksStatus.Values.ToArray();
        return (backupIds, backupStatuses);
    }

    public void DeleteBackup(Guid backupId)
    {
        if (_backupTasksStatus.TryGetValue(backupId, out TaskStatus<BackupMetadataDTO>? status))
        {
            if (status.State == TaskState.Processing)
                throw new HttpResponseException(400, "Backup is still processing. Cannot delete.");

            var path = Path.Combine("/backup/", backupId.ToString() + ".orbak");
            File.Delete(path);
            _backupTasksStatus.TryRemove(backupId, out _);
        }
        else
        {
            throw new HttpResponseException(404, "Backup not found");
        }
    }

    public void RestoreBackupAsync(string filePath)
    {
        lock (_lock)
        {
            if (_restoreTaskStatus != null && _restoreTaskStatus.State == TaskState.Processing)
                throw new HttpResponseException(400, "Another restore task is already in progress.");

            if (_backupTasksStatus.ToImmutableDictionary().Values.Any(status => status.State == TaskState.Processing))
                throw new HttpResponseException(400, "Cannot start restore while a backup task is in progress.");

            _restoreTaskStatus = new() { State = TaskState.Processing };
        }

        Task.Run(async () =>
        {
            try
            {
                var safetyBackupPath = Path.GetTempFileName();
                await _sibyllaManager.BackupFacts(safetyBackupPath);
    
                try
                {
                    await _sibyllaManager.RestoreFacts(filePath, (current, total) =>
                    {
                        var progress = (float)current / total;
                        _restoreTaskStatus.Progress = progress;
                        return (int)(progress * 100);
                    });
                    _restoreTaskStatus.State = TaskState.Completed;
                    _restoreTaskStatus.Progress = 1.0f;
                }
                catch (Exception)
                {
                    await _sibyllaManager.RestoreFacts(safetyBackupPath);
                    throw;
                }
            }
            catch (Exception)
            {
                _restoreTaskStatus.State = TaskState.Failed;
            }
        });
    }

    public TaskStatus? CheckRestoreStatus()
    {
        return _restoreTaskStatus;
    }

    private int UpdateBackupProgress(Guid backupId, int current, int total)
    {
        var statusPercentage = (float)current / total;
        if (_backupTasksStatus.TryGetValue(backupId, out TaskStatus<BackupMetadataDTO>? status))
        {
            status.Progress = statusPercentage;
        }

        return (int)(statusPercentage * 100);
    }
}