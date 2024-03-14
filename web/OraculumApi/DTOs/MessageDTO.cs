
namespace OraculumApi.Models.FrontOffice
{
    public partial class MessageDTO : IDTO<Message>
    {
        public Guid Id { get; set; }
        public string? Sender { get; set; }
        public string? Text { get; set; }
        public string[]? FactIds { get; set; }
        public string[]? ExtraFactIds { get; set; }
        public DateTime? Timestamp { get; set; }

        public Message toEntity()
        {
            return new Message()
            {
                id = this.Id,
                sender = this.Sender,
                text = this.Text,
                factIds = this.FactIds,
                extraFactIds = this.ExtraFactIds,
                timestamp = this.Timestamp
            };
        }
    }
}
