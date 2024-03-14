
namespace OraculumApi.Models.FrontOffice
{
    public partial class FeedbackDTO : IDTO<Feedback>
    {
        public Guid Id { get; set; }
        public string? Text { get; set; }
        public string? Rating { get; set; }
        public string? SibyllaId { get; set; }
        public string? ChatId { get; set; }
        public string? MessageId { get; set; }
        public string? QuestionMessageId { get; set; }
        public string? FactId { get; set; }
        public DateTime Timestamp { get; set; }

        public Feedback toEntity()
        {
            return new Feedback()
            {
                id = this.Id,
                text = this.Text,
                rating = this.Rating,
                sibyllaId = this.SibyllaId,
                chatId = this.ChatId,
                messageId = this.MessageId,
                questionMessageId = this.QuestionMessageId,
                factId = this.FactId,
                timestamp = this.Timestamp
            };
        }
    }
}
