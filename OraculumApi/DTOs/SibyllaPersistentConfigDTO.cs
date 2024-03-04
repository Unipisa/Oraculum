using WeaviateNET;

namespace OraculumApi.Models
{
    [IndexNullState]
    public partial class SibyllaPersistentConfigDTO : IDTO<SibyllaPersistentConfig>
    {
        public Guid Id { get; set;}
        public required string Name { get; set;}
        public required string ConfigJSON { get; set;}

        public SibyllaPersistentConfig toEntity(){
            return new SibyllaPersistentConfig(){
                id = this.Id,
                name = this.Name,
                configJSON = this.ConfigJSON
            };
        }
    }
}
