using WeaviateNET;

namespace OraculumApi.Models
{
    [IndexNullState]
    public partial class SibyllaPersistentConfig : WeaviateEntity, IEntity<SibyllaPersistentConfigDTO>
    {
        public string? name;
        public  string? configJSON;

        public SibyllaPersistentConfigDTO toDTO(){
            return new SibyllaPersistentConfigDTO(){
                Id = this.id,
                Name = this.name ?? "",
                ConfigJSON = this.configJSON ?? ""
            };
        }
    }
}
