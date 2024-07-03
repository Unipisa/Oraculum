using OpenAI.Managers;
using Oraculum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace OraculumCLI
{
    public enum GPTProvider
    {
        OpenAI,
        Azure
    }

    public class OraculumConfiguration
    {
        public static OraculumConfiguration? FromJson(string json)
        {
            return System.Text.Json.JsonSerializer.Deserialize<OraculumConfiguration>(json);
        }

        public string? WeaviateEndpoint { get; set; }
        public string? WeaviateApiKey { get; set; }
        public GPTProvider GPTProvider { get; set; } = GPTProvider.OpenAI;
        public string? LocalProvider { get; set; } = null;
        public string? OpenAIApiKey { get; set; }
        public string? OpenAIOrgId { get; set; }
        public string? AzureApiKey { get; set; }
        public string? AzureResourceName { get; set; }
        public string? AzureDeploymentId { get; set; }
        public string? UserName { get; set; }
    }

    [Cmdlet(VerbsCommunications.Connect, "Oraculum")]
    public class ConnectOraculum : PSCmdlet
    {
        [Parameter]
        public string? ConfigFile { get; set; }

        [Parameter]
        public OraculumConfiguration Config { get; set; } = null!;

        protected override void ProcessRecord()
        {
            if (ConfigFile != null)
            {
                var json = System.IO.File.ReadAllText(ConfigFile);
                Config = OraculumConfiguration.FromJson(json)!;
            } else if (Config == null)
            {
                throw new Exception("Either ConfigFile or Config must be set");
            }

            var config = new Oraculum.Configuration()
            {
                WeaviateEndpoint = Config.WeaviateEndpoint,
                WeaviateApiKey = Config.WeaviateApiKey,
                LocalProvider = Config.LocalProvider,
                OpenAIApiKey = Config.OpenAIApiKey,
                OpenAIOrgId = Config.OpenAIOrgId,
                AzureOpenAIApiKey = Config.AzureApiKey,
                AzureResourceName = Config.AzureResourceName,
                AzureDeploymentId = Config.AzureDeploymentId,
                Provider = Config.GPTProvider == GPTProvider.Azure ? OpenAI.ProviderType.Azure : OpenAI.ProviderType.OpenAi,
                UserName = Config.UserName
            };
            var oraculum = new Oraculum.Oraculum(config);
            var j = oraculum.IsKBInitialized();
            j.Wait();
            if (j.Result)
              oraculum.Connect().Wait();

            var openai = config.CreateOpenAIService();

            SessionState.PSVariable.Set("Oraculum", oraculum);
            SessionState.PSVariable.Set("OpenAIService", openai);
            WriteObject(oraculum);
        }
    }
}
