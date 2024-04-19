using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Oraculum;
using System.Diagnostics.CodeAnalysis;


namespace OraculumApi.Models.BackOffice
{
    /// <summary>
    /// Represents the configuration DTO for Sibylla.
    /// </summary>
    [DataContract]
    public partial class SibyllaConfigDto : SibyllaConf
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SibyllaConfigDto"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the configuration</param>
        [SetsRequiredMembers]
        public SibyllaConfigDto(string id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets or Sets Id
        /// </summary>
        [Required]
        [DataMember(Name = "id")]
        [JsonProperty]
        public required string Id { get; set; }

        public static SibyllaConfigDto ToSibyllaConfigDto(SibyllaConf sibyllaConf)
        {
            //TODO:  the constructor with the Id is required until we dont use a db for storing configurations
            SibyllaConfigDto SibyllaConfigDto = new SibyllaConfigDto(sibyllaConf.Title);
            SibyllaConfigDto.BaseSystemPrompt = sibyllaConf.BaseSystemPrompt;
            SibyllaConfigDto.BaseAssistantPrompt = sibyllaConf.BaseAssistantPrompt;
            SibyllaConfigDto.MaxTokens = sibyllaConf.MaxTokens;
            SibyllaConfigDto.Model = sibyllaConf.Model;
            SibyllaConfigDto.Temperature = sibyllaConf.Temperature;
            SibyllaConfigDto.TopP = sibyllaConf.TopP;
            SibyllaConfigDto.FrequencyPenalty = sibyllaConf.FrequencyPenalty;
            SibyllaConfigDto.PresencePenalty = sibyllaConf.PresencePenalty;
            SibyllaConfigDto.MemoryConfiguration = sibyllaConf.MemoryConfiguration;
            // SibyllaConfigDto.OutOfScopeTag = sibyllaConf.OutOfScopeTag;
            // SibyllaConfigDto.Limit = sibyllaConf.Limit;
            return SibyllaConfigDto;
        }

        public static SibyllaConf FromSibyllaConfigDto(SibyllaConfigDto sibyllaConfigDto)
        {
            SibyllaConf SibyllaConf = new SibyllaConf();
            SibyllaConf.Title = sibyllaConfigDto.Title;
            SibyllaConf.BaseSystemPrompt = sibyllaConfigDto.BaseSystemPrompt;
            SibyllaConf.BaseAssistantPrompt = sibyllaConfigDto.BaseAssistantPrompt;
            SibyllaConf.MaxTokens = sibyllaConfigDto.MaxTokens;
            SibyllaConf.Model = sibyllaConfigDto.Model;
            SibyllaConf.Temperature = sibyllaConfigDto.Temperature;
            SibyllaConf.TopP = sibyllaConfigDto.TopP;
            SibyllaConf.FrequencyPenalty = sibyllaConfigDto.FrequencyPenalty;
            SibyllaConf.PresencePenalty = sibyllaConfigDto.PresencePenalty;
            SibyllaConf.MemoryConfiguration = sibyllaConfigDto.MemoryConfiguration;
            // SibyllaConf.OutOfScopeTag = sibyllaConfigDto.OutOfScopeTag;
            // SibyllaConf.Limit = sibyllaConfigDto.Limit;
            return SibyllaConf;
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
