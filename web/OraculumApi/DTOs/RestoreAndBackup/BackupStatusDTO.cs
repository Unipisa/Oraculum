using WeaviateNET;

namespace OraculumApi.Models.RestoreAndBackup
{
    [IndexNullState]
    public class BackupStatusDTO
    {
        public Guid id { get; set; }
        public TaskStatus<BackupMetadataDTO> status { get; set; }
    }
}