using Oraculum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OraculumCLI
{
    [Cmdlet(VerbsCommon.New, "SibyllaConf")]
    public class NewSibyllaConf : OraculumPSCmdlet
    {
        [Parameter(Mandatory=true)]
        public string? SystemPrompt { get; set; }

        [Parameter(Mandatory=true)]
        public string? AssistantPrompt { get; set; }

        [Parameter]
        public int MaxTokens { get; set; } = 1024;
        [Parameter]
        public string Model { get; set; } = "gpt-3.5-turbo";
        [Parameter]
        public float Temperature { get; set; } = 0.2f;
        [Parameter]
        public float TopP { get; set; } = 1.0f;
        [Parameter]
        public float FrequencyPenalty { get; set; } = 0.0f;
        [Parameter]
        public float PresencePenalty { get; set; } = 0.0f;

        [Parameter]
        public string? File { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            var c = new SibyllaConf()
            {
                BaseSystemPrompt = SystemPrompt,
                BaseAssistantPrompt = AssistantPrompt,
                MaxTokens = MaxTokens,
                Model = Model,
                Temperature = Temperature,
                TopP = TopP,
                FrequencyPenalty = FrequencyPenalty,
                PresencePenalty = PresencePenalty
            };
            if (File != null)
            {
                System.IO.File.WriteAllText(File, JsonSerializer.Serialize(c));
            }
            WriteObject(c);
        }
    }
}
