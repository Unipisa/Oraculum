
namespace OraculumApi.Models.FrontOffice
{
    public partial class Message : WeaviateEntity, IEntity<MessageDTO>
    {
        public string? sender;
        public string? text;
        public string[]? factIds;
        public string[]? extraFactIds;
        public DateTime? timestamp;

        public MessageDTO toDTO()
        {
            return new MessageDTO()
            {
                Id = this.id,
                Sender = this.sender,
                Text = this.text,
                FactIds = this.factIds,
                ExtraFactIds = this.extraFactIds,
                Timestamp = this.timestamp
            };
        }
    }
}
