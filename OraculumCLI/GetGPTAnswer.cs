using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.Tokenizer.GPT3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace OraculumCLI
{
    [Cmdlet(VerbsCommon.Get, "GPTAnswer")]
    public class GetGPTAnswer : OraculumPSCmdlet
    {
        [Parameter]
        public string Model { get; set; } = Models.Gpt_3_5_Turbo;

        [Parameter]
        public int MaxTokens { get; set; } = 150;

        [Parameter]
        public float Temperature { get; set; } = 0.1f;

        [Parameter]
        public string? SystemPrompt { get; set; } = null;

        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0)]
        public string? Prompt { get; set; } = null;

        [Parameter]
        public bool OutFullObject { get; set; } = false;

        protected override void ProcessRecord()
        {
            var msg = new List<ChatMessage>();
            if (SystemPrompt != null)
            {
                msg.Add(new ChatMessage("system", SystemPrompt));
            }
            msg.Add(new ChatMessage("user", Prompt!));
            var j = OpenAIConnection.ChatCompletion.CreateCompletion(new OpenAI.ObjectModels.RequestModels.ChatCompletionCreateRequest()
            {
                MaxTokens = MaxTokens,
                Temperature = Temperature,
                Model = Model,
                Messages = msg
            });
            j.Wait();
            if (j.Result.Successful)
            {
                WriteObject(OutFullObject ? j.Result : j.Result.Choices[0].Message.Content);
                return;
            }
            WriteObject(j.Result.Error);
        }
    }
}
