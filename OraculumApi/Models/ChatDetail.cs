using WeaviateNET;

namespace OraculumApi.Models
{
    [IndexNullState]
    public partial class ChatDetail : WeaviateEntity, IEntity<ChatDetailDTO>
    {
        public string? name;
        public  string? sibyllaId;
        public string? sibyllaRef;
        public string[]? messagesIds;
        public DateTime? creationTimestamp;

        public ChatDetailDTO toDTO(){
            return new ChatDetailDTO(){
                Id = this.id,
                Name = this.name,
                SibyllaId = this.sibyllaId,
                SibyllaRef = this.sibyllaRef,
                MessagesIds = this.messagesIds,
                CreationTimestamp = this.creationTimestamp
            };
        }
    }
}
