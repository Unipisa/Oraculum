using Oraculum;

namespace OraculumApi.Models.BackOffice
{
    public class SibyllaInfoDto
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? BaseAssistantPrompt { get; set; }
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
