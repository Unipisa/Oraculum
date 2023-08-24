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
        public Oraculum.Oraculum? sibylla;


        [TestInitialize]
        public void init()
        {
            IConfigurationBuilder config = new ConfigurationBuilder()
                .AddUserSecrets<SchemaTest>();
            var conf = config.Build();

            sibylla = new Oraculum.Oraculum(new Configuration()
            {
                WeaviateApiKey = conf["Weaviate:ApiKey"],
                WeaviateEndpoint = conf["Weaviate:ServiceEndpoint"],
                OpenAIApiKey = conf["OpenAI:ApiKey"],
                OpenAIOrgId = conf["OpenAI:OrgId"]
            });
            sibylla.Connect().Wait();
        }

        [TestMethod]
        public async Task AddFactTest()
        {
            Assert.IsNotNull(sibylla);
            await sibylla.AddFact(new Fact()
            {
                factType = "faq",
                category = "missioni",
                title = "Question",
                content = "This is a test fact.",
                expiration = DateTime.Now.AddYears(100)
            });
        }

    }
}
