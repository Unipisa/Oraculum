using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Oraculum;
using System.Diagnostics.CodeAnalysis;


namespace OraculumApi.Models.BackOffice
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public partial class SibyllaConfigDto : SibyllaConf
    {
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
            SibyllaConfigDto.FactFilter = sibyllaConf.FactFilter;
            SibyllaConfigDto.CategoryFilter = sibyllaConf.CategoryFilter;
            SibyllaConfigDto.TagFilter = sibyllaConf.TagFilter;
            SibyllaConfigDto.FactMemoryTTL = sibyllaConf.FactMemoryTTL;
            SibyllaConfigDto.MemorySpan = sibyllaConf.MemorySpan;
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
            SibyllaConf.FactFilter = sibyllaConfigDto.FactFilter;
            SibyllaConf.CategoryFilter = sibyllaConfigDto.CategoryFilter;
            SibyllaConf.TagFilter = sibyllaConfigDto.TagFilter;
            SibyllaConf.FactMemoryTTL = sibyllaConfigDto.FactMemoryTTL;
            SibyllaConf.MemorySpan = sibyllaConfigDto.MemorySpan;
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
