using WeaviateNET;

namespace OraculumApi.Models.RestoreAndBackup
{
    [IndexNullState]
    public class BackupIdDTO
    {
        public Guid BackupId { get; set; }
    }
}