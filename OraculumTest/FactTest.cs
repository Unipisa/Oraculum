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
                OpenAIApiKey = conf["OpenAI:ApiKey"],
                OpenAIOrgId = conf["OpenAI:OrgId"]
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
            var facts = await oraculum.FindRelevantFacts("Cosa sono i codici PAST?", limit: 5, distance: 0.23, factTypeFilter: new[]{ "faq" });
            Assert.IsNotNull(facts);
        }

    }
}
