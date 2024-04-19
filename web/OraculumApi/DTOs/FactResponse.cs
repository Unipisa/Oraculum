using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace OraculumApi.Models.BackOffice
{
    public class FactResponse
    {
        public string Content { get; set; }
        public string Title { get; set; }
        public string Citation { get; set; }
        public string FactType { get; set; }
    }

    public class FactWrapper
    {
        public List<FactResponse> Facts { get; set; }
    }

    public class FactConverter
    {
        public static List<Fact> ConvertToFactList(string json)
        {
            var factResponses = JsonConvert.DeserializeObject<List<FactResponse>>(json);
            var facts = new List<Fact>();

            foreach (var factResponse in factResponses)
            {
                var fact = new Fact
                {
                    Id = Guid.NewGuid(), // Puoi generare l'ID come richiesto
                    FactType = factResponse.FactType,
                    Category = "", // Aggiungi la logica per determinare la categoria
                    Tags = new List<string>(), // Inizializza come necessario
                    Title = factResponse.Title,
                    Content = factResponse.Content,
                    Citation = factResponse.Citation,
                    Reference = null, // Imposta se necessario
                    Expiration = null, // Imposta se necessario
                    OutOfLimit = null, // Imposta se necessario
                };

                facts.Add(fact);
            }

            return facts;
        }
    }
}
