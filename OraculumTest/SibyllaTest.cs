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
    public class SibyllaTest
    {
        private Sibylla? sibylla;

        [TestInitialize]
        public void init()
        {
            IConfigurationBuilder config = new ConfigurationBuilder()
                .AddUserSecrets<SchemaTest>();
            var conf = config.Build();

            var sconf = new SibyllaConf()
            {
                BaseSystemPrompt = "Sei un operatore che risponde alle domande degli utenti del sistema UWeb missioni dell'Università di Pisa. Per rispondere userai solo fatti veri e le informazioni della FAQ al posto della tua conoscenza. Ciascuna domanda che riceverai sarà quella di un utente che ha un problema. Rispondi nella lingua in cui ti viene posta la domanda. Userai la conoscenza nei dati Xml di seguito.",
                BaseAssistantPrompt = "Salve, sono qui per assisterti sulla procedura delle missioni di Ateneo. Hello, I'm here to assist you on the Ateneo missions procedure, ask in any language and I will do my best to answer.",
                MaxTokens = 1024
            };

            sibylla = new Sibylla(new Configuration()
            {
                WeaviateApiKey = conf["Weaviate:ApiKey"],
                WeaviateEndpoint = conf["Weaviate:ServiceEndpoint"],
                OpenAIApiKey = conf["OpenAI:ApiKey"],
                OpenAIOrgId = conf["OpenAI:OrgId"]
            }, sconf);
            
            sibylla.Connect().Wait();
        }

        [TestMethod]
        public async Task TestQuestion1()
        {
            Assert.IsNotNull(sibylla);
            var answer = await sibylla.Answer("Cosa sono i codici PAST?", new KnowledgeFilter()
            {
                CategoryFilter = new[] { "missioni" }
            });
            //var answer = await sibylla.Answer("What are PAST codes and which one should I use?");
            Assert.IsNotNull(answer);
        }
    }
}
