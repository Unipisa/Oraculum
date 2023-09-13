using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace OraculumCLI
{
    /// <summary>
    /// Cmdlet to create a new Oraculum configuration object
    /// </summary>
    [Cmdlet(VerbsCommon.New, "OraculumConf")]
    public class NewOraculumConf : OraculumPSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string? WeaviateEndpoint { get; set; }

        [Parameter]
        public string? WeaviateApiKey { get; set; }

        [Parameter(Mandatory = true)]
        public string? OpenAIApiKey { get; set; }

        [Parameter]
        public string? OpenAIOrgId { get; set; }

        protected override void ProcessRecord()
        {
            var config = new OraculumConfiguration()
            {
                WeaviateEndpoint = WeaviateEndpoint,
                WeaviateApiKey = WeaviateApiKey,
                OpenAIApiKey = OpenAIApiKey,
                OpenAIOrgId = OpenAIOrgId
            };
            WriteObject(config);
        }
    }
}
