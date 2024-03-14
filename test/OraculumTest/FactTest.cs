using Microsoft.Extensions.Configuration;
using Oraculum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OraculumTest
{
    [TestClass]
    public class FactTest
    {
        public Oraculum.Oraculum? oraculum;


        [TestInitialize]
        public void init()
        {
            IConfigurationBuilder config = new ConfigurationBuilder()
                .AddUserSecrets<SchemaTest>();
            var conf = config.Build();

            oraculum = new Oraculum.Oraculum(new Configuration()
            {
                WeaviateApiKey = conf["Weaviate:ApiKey"],
                WeaviateEndpoint = conf["Weaviate:ServiceEndpoint"],
                Provider = conf["GPTProvider"] == "Azure" ? OpenAI.ProviderType.Azure : OpenAI.ProviderType.OpenAi,
                OpenAIApiKey = conf["OpenAI:ApiKey"],
                OpenAIOrgId = conf["OpenAI:OrgId"],
                AzureOpenAIApiKey = conf["Azure:ApiKey"],
                AzureResourceName = conf["Azure:ResourceName"],
                AzureDeploymentId = conf["Azure:DeploymentId"]
            });
            oraculum.Connect().Wait();
        }

        [TestMethod]
        public async Task AddFactTest()
        {
            Assert.IsNotNull(oraculum);
            var id = await oraculum.AddFact(new Fact()
            {
                factType = "test",
                category = "missioni",
                title = "Question",
                content = "This is a test fact.",
                expiration = DateTime.Now.AddYears(100)
            });

            Assert.IsNotNull(id);
            Assert.IsTrue(await oraculum.DeleteFact(id.Value));
        }

        [TestMethod]
        public async Task TestRelevantFacts()
        {
            Assert.IsNotNull(oraculum);
            var facts = await oraculum.FindRelevantFacts("Cosa sono i codici PAST?", new FactFilter()
            {
                Limit = 5,
                Distance = 0.23f,
                FactTypeFilter = new[] { "faq" }
            });
            Assert.IsNotNull(facts);
        }

        [TestMethod]
        public async Task TestDistanceCheck()
        {
            Assert.IsNotNull(oraculum);
            var facts = (await oraculum.FindRelevantFacts("Cosa sono i codici PASTG?", new FactFilter()
            {
                AutocutPercentage = 0.5f,
                Limit = 100
            })).ToArray();
            var min = facts[0].distance;
            var max = facts[facts.Length - 1].distance;
            var norm = (from f in facts select (f.distance - min) / (max - min)).ToArray();
            Assert.IsTrue(norm[0] < 0.1f);
        }

    }
}
