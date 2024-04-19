using Oraculum;

namespace OraculumApi.Models.BackOffice
{
    public class SibyllaInfoDto
    {
        /// <summary>
        /// Id of the Sibylla
        /// </summary>
        /// <example>211f0109-c78b-4116-a3ff-d5429af77185</example>
        public string? Id { get; set; }

        /// <summary>
        /// Visible title
        /// </summary>
        /// <example>My custom Assistant</example>
        public string? Title { get; set; }

        /// <summary>
        /// Welcome message of the Sibylla
        /// </summary>
        /// <example>Hello, I'm Custom Assistant, how can I help you?</example>
        public string? BaseAssistantPrompt { get; set; }

        /// <summary>
        /// If a sibylla is hidden in sibylla selection page or not
        /// </summary>
        public bool? Hidden { get; set; }

        public static SibyllaInfoDto FromSibyllaConf(SibyllaConf sibyllaConf)
        {
            SibyllaInfoDto sibyllaInfoDto = new SibyllaInfoDto()
            {
                Id = sibyllaConf.SibyllaName,
                Title = sibyllaConf.Title,
                BaseAssistantPrompt = sibyllaConf.BaseAssistantPrompt,
                Hidden = sibyllaConf.Hidden,
            };
            return sibyllaInfoDto;
        }
    }

}
