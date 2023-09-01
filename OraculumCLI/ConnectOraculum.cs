using Oraculum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace OraculumCLI
{
    public class OraculumConfiguration
    {
        public static OraculumConfiguration? FromJson(string json)
        {
            return System.Text.Json.JsonSerializer.Deserialize<OraculumConfiguration>(json);
        }

        public string? WeaviateEndpoint { get; set; }
        public string? WeaviateApiKey { get; set; }
        public string? OpenAIApiKey { get; set; }
        public string? OpenAIOrgId { get; set; }
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

            var config = new Configuration()
            {
                WeaviateEndpoint = Config.WeaviateEndpoint,
                WeaviateApiKey = Config.WeaviateApiKey,
                OpenAIApiKey = Config.OpenAIApiKey,
                OpenAIOrgId = Config.OpenAIOrgId
            };
            var oraculum = new Oraculum.Oraculum(config);
            var j = oraculum.IsKBInitialized();
            j.Wait();
            if (j.Result)
              oraculum.Connect().Wait();

            SessionState.PSVariable.Set("Oraculum", oraculum);
            WriteObject(oraculum);
        }
    }
}
