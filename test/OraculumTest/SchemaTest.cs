//#define CLEARDB

using Microsoft.Extensions.Configuration;
using Oraculum;
using System.Dynamic;
using WeaviateNET;

namespace OraculumTest
{
    [TestClass]
    public class SchemaTest
    {
        public Oraculum.Oraculum? oraculum;


        [TestInitialize]
        public void init()
        {
            IConfigurationBuilder config = new ConfigurationBuilder()
                .AddUserSecrets<SchemaTest>();
            var conf = config.Build();

            oraculum = new Oraculum.Oraculum(new Configuration() { 
                WeaviateApiKey = conf["Weaviate:ApiKey"],
                WeaviateEndpoint = conf["Weaviate:ServiceEndpoint"],
                Provider = conf["GPTProvider"] == "Azure" ? OpenAI.ProviderType.Azure : OpenAI.ProviderType.OpenAi,
                OpenAIApiKey = conf["OpenAI:ApiKey"],
                OpenAIOrgId = conf["OpenAI:OrgId"],
                AzureOpenAIApiKey = conf["Azure:ApiKey"],
                AzureResourceName = conf["Azure:ResourceName"],
                AzureDeploymentId = conf["Azure:DeploymentId"],
                UserName = conf["UserName"]
            });
        }

        [TestMethod]
        public async Task ClearDBTest()
        {
#if CLEARDB
            Assert.IsNotNull(oraculum);
            await oraculum.UpgradeDB();
            await oraculum.Init();
#else
            Assert.IsNotNull(oraculum);
            await oraculum.Connect();
            Assert.IsTrue(true);
#endif
        }

        [TestMethod]
        public async Task TestUpgradeDB()
        {
            Assert.IsNotNull(oraculum);
            await oraculum.UpgradeDB();
        }

        [TestMethod]
        public async Task TestConnectAndInit()
        {
            Assert.IsNotNull(oraculum);
            if (await oraculum.IsKBInitialized())
            {
                await oraculum.Connect();
            } else
            {
                await oraculum.Init();
                await oraculum.Connect();
            }
        }

        [TestMethod]
        public async Task TestCount()
        {
            Assert.IsNotNull(oraculum);
            if (!oraculum.IsConnected)
                await oraculum.Connect();

            Assert.IsTrue(await oraculum.TotalFacts() >= 0);
        }

        [TestMethod]
        public async Task TestListFilteredFacts()
        {
            Assert.IsNotNull(oraculum);
            if (!oraculum.IsConnected)
                await oraculum.Connect();
            var facts = await oraculum.ListFilteredFacts(new FactFilter() { CategoryFilter = new string[] { "test" } });
            Assert.IsTrue(facts.Count() >= 0);
        }

    }
}