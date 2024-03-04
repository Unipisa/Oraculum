
namespace OraculumApi.Models.FrontOffice
{
    public partial class Feedback : WeaviateEntity, IEntity<FeedbackDTO>
    {
        public string? text;
        public string? rating;
        public string? sibyllaId;
        public string? chatId;
        public string? messageId;
        public string? questionMessageId;
        public string? factId;
        public DateTime timestamp;

        public FeedbackDTO toDTO()
        {
            return new FeedbackDTO()
            {
                Id = this.id,
                Text = this.text,
                Rating = this.rating,
                SibyllaId = this.sibyllaId,
                ChatId = this.chatId,
                MessageId = this.messageId,
                QuestionMessageId = this.questionMessageId,
                FactId = this.factId,
                Timestamp = this.timestamp
            };
        }
    }
}
