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

        [Parameter]
        public GPTProvider GPTProvider { get; set; } = GPTProvider.OpenAI;

        [Parameter]
        public string? OpenAIApiKey { get; set; }

        [Parameter]
        public string? OpenAIOrgId { get; set; }

        [Parameter]
        public string? AzureApiKey { get; set; }

        [Parameter]
        public string? AzureResourceName { get; set; }

        [Parameter]
        public string? AzureDeploymentId { get; set; }

        protected override void ProcessRecord()
        {
            var config = new OraculumConfiguration()
            {
                WeaviateEndpoint = WeaviateEndpoint,
                WeaviateApiKey = WeaviateApiKey,
                GPTProvider = GPTProvider.OpenAI,
                OpenAIApiKey = OpenAIApiKey,
                OpenAIOrgId = OpenAIOrgId,
                AzureApiKey = AzureApiKey,
                AzureResourceName = AzureResourceName,
                AzureDeploymentId = AzureDeploymentId
            };
            WriteObject(config);
        }
    }
}
