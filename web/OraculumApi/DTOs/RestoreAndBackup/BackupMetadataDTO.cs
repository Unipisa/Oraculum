using WeaviateNET;

namespace OraculumApi.Models.RestoreAndBackup
{
    [IndexNullState]
    public partial class BackupMetadataDTO
    {
        public DateTime CreationTime { get; set;}
    }
}