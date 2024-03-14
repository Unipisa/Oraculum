using WeaviateNET;
using OraculumApi.Models.FrontOffice;

namespace OraculumApi.Models
{
    [IndexNullState]
    public partial class ChatDetailDTO : IDTO<ChatDetail>
    {
        public Guid Id { get; set;}
        public string? Name { get; set;}
        public  string? SibyllaId { get; set;}
        public string? SibyllaRef { get; set;}
        public string[]? MessagesIds { get; set;}
        public List<MessageDTO>? Messages { get; set;}
        public Boolean IsActive { get; set; }
        public DateTime? CreationTimestamp { get; set;}

        public ChatDetail toEntity(){
            return new ChatDetail(){
                id = this.Id,
                name = this.Name,
                sibyllaId = this.SibyllaId,
                sibyllaRef = this.SibyllaRef,
                messagesIds = this.MessagesIds,
                creationTimestamp = this.CreationTimestamp
            };
        }
    }
}
