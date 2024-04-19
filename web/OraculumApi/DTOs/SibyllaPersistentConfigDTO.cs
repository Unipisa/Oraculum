using Newtonsoft.Json;
using Oraculum;
using OraculumApi.Models.BackOffice;
using WeaviateNET;

namespace OraculumApi.Models
{
    /// <summary>
    /// Represents a persistent configuration for Sibylla.
    /// </summary>
    [IndexNullState]
    public partial class SibyllaPersistentConfigDTO : SibyllaConf, IDTO<SibyllaPersistentConfig>
    {
        /// <summary>
        /// Gets or sets the unique identifier for the configuration.
        /// </summary>
        /// <value>The configuration's unique identifier.</value>
        public Guid Id { get; set;}

        /// <summary>
        /// Gets or sets the name of the configuration.
        /// Required.
        /// </summary>
        /// <value>The name of the configuration.</value>
        public required string Name { get; set;}


        /// <summary>
        /// Converts this DTO to its entity counterpart, <see cref="SibyllaPersistentConfig"/>.
        /// </summary>
        /// <returns>The <see cref="SibyllaPersistentConfig"/> entity.</returns>
        public SibyllaPersistentConfig toEntity(){
            return new SibyllaPersistentConfig(){
                id = Id,
                name = Name,
                configJSON = JsonConvert.SerializeObject(new {
                    Title,
                    MemoryConfiguration,
                    BaseSystemPrompt,
                    BaseAssistantPrompt,
                    MaxTokens,
                    Model,
                    Temperature,
                    TopP,
                    FrequencyPenalty,
                    PresencePenalty,
                    OutOfScopePrefix,
                    FunctionsDefaultAnswerHook,
                    FunctionsBeforeAnswerHook,
                    SibyllaName,
                    Hidden
                })
            };
        }
    }
}
