using OpenAI.Tokenizer.GPT3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace OraculumCLI
{
    [Cmdlet(VerbsCommon.Get, "TokenCount")]
    public class GetTokenCount : OraculumPSCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0)]
        public string? Text { get; set; }

        protected override void ProcessRecord()
        {
            if (Text == null)
            {
                WriteObject(false);
                return;
            }

            WriteObject(TokenizerGpt3.TokenCount(Text));
        }
    }
}
