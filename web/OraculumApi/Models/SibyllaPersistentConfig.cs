using Newtonsoft.Json;
using Oraculum;
using WeaviateNET;

namespace OraculumApi.Models
{
    [IndexNullState]
    public partial class SibyllaPersistentConfig : WeaviateEntity, IEntity<SibyllaPersistentConfigDTO>
    {
        public string? name;
        public  string? configJSON;

        public SibyllaPersistentConfigDTO toDTO(){
            SibyllaConf parsedConfig = JsonConvert.DeserializeObject<SibyllaConf>(configJSON!) ?? new SibyllaConf();

            return new SibyllaPersistentConfigDTO(){
                Id = id,
                Name = name ?? "",
                Title = parsedConfig.Title,
                MemoryConfiguration = parsedConfig.MemoryConfiguration,
                BaseSystemPrompt = parsedConfig.BaseSystemPrompt,
                BaseAssistantPrompt = parsedConfig.BaseAssistantPrompt,
                MaxTokens = parsedConfig.MaxTokens,
                Model = parsedConfig.Model,
                Temperature = parsedConfig.Temperature,
                TopP = parsedConfig.TopP,
                FrequencyPenalty = parsedConfig.FrequencyPenalty,
                PresencePenalty = parsedConfig.PresencePenalty,
                OutOfScopePrefix = parsedConfig.OutOfScopePrefix,
                FunctionsDefaultAnswerHook = parsedConfig.FunctionsDefaultAnswerHook,
                FunctionsBeforeAnswerHook = parsedConfig.FunctionsBeforeAnswerHook,
                SibyllaName = parsedConfig.SibyllaName,
                Hidden = parsedConfig.Hidden
            };
        }
    }
}
