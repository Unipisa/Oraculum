﻿using Oraculum;
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
        public static OraculumConfiguration FromJson(string json)
        {
            return System.Text.Json.JsonSerializer.Deserialize<OraculumConfiguration>(json);
        }

        public string? WeaviateEndpoint { get; set; }
        public string? WeaviateApiKey { get; set; }
        public string? OpenAIApiKey { get; set; }
        public string? OpenAIOrgId { get; set; }
    }

    [Cmdlet(VerbsCommunications.Connect, "Oraculum")]
    public class ConnectOraculum : Cmdlet
    {
        [Parameter(Mandatory = true)]
        public OraculumConfiguration Config { get; set; } = null!;

        protected override void ProcessRecord()
        {
            var config = new Configuration()
            {
                WeaviateEndpoint = Config.WeaviateEndpoint,
                WeaviateApiKey = Config.WeaviateApiKey,
                OpenAIApiKey = Config.OpenAIApiKey,
                OpenAIOrgId = Config.OpenAIOrgId
            };
            var sibylla = new Oraculum.Oraculum(config);
            sibylla.Connect().Wait();
            WriteObject(sibylla);
        }
    }
}