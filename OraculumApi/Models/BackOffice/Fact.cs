using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace OraculumApi.Models.BackOffice
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public partial class Fact
    {
        /// <summary>
        /// Gets or Sets Id
        /// </summary>

        [DataMember(Name = "id")]
        public required Guid Id { get; set; }

        /// <summary>
        /// Type of the fact
        /// </summary>
        /// <value>Type of the fact</value>
        [Required]

        [DataMember(Name = "factType")]
        public required string FactType { get; set; }

        /// <summary>
        /// Category of the fact
        /// </summary>
        /// <value>Category of the fact</value>

        [DataMember(Name = "category")]
        public required string Category { get; set; }

        /// <summary>
        /// Tags associated with the fact
        /// </summary>
        /// <value>Tags associated with the fact</value>

        [DataMember(Name = "tags")]
        public required List<string> Tags { get; set; }

        /// <summary>
        /// Title of the fact
        /// </summary>
        /// <value>Title of the fact</value>

        [DataMember(Name = "title")]
        public required string Title { get; set; }

        /// <summary>
        /// Main content or body of the fact
        /// </summary>
        /// <value>Main content or body of the fact</value>
        [Required]

        [DataMember(Name = "content")]
        public required string Content { get; set; }

        /// <summary>
        /// Citation associated with the fact
        /// </summary>
        /// <value>Citation associated with the fact</value>

        [DataMember(Name = "citation")]
        public required string Citation { get; set; }

        /// <summary>
        /// Reference for the fact
        /// </summary>
        /// <value>Reference for the fact</value>

        [DataMember(Name = "reference")]
        public required string Reference { get; set; }

        /// <summary>
        /// Expiration date of the fact
        /// </summary>
        /// <value>Expiration date of the fact</value>

        [DataMember(Name = "expiration")]
        public DateTime? Expiration { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class Fact {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  FactType: ").Append(FactType).Append("\n");
            sb.Append("  Category: ").Append(Category).Append("\n");
            sb.Append("  Tags: ").Append(Tags).Append("\n");
            sb.Append("  Title: ").Append(Title).Append("\n");
            sb.Append("  Content: ").Append(Content).Append("\n");
            sb.Append("  Citation: ").Append(Citation).Append("\n");
            sb.Append("  Reference: ").Append(Reference).Append("\n");
            sb.Append("  Expiration: ").Append(Expiration).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public static Fact FromOraculumFact(Oraculum.Fact oraculumFact)
        {
            return new Fact
            {
                Id = oraculumFact.id ?? Guid.Empty,
                FactType = oraculumFact.factType ?? "",
                Category = oraculumFact.category ?? "",
                Tags = oraculumFact.tags != null ? oraculumFact.tags.ToList() : new List<string>(),
                Title = oraculumFact.title ?? "",
                Content = oraculumFact.content ?? "",
                Citation = oraculumFact.citation ?? "",
                Reference = oraculumFact.reference ?? "",
                Expiration = oraculumFact.expiration
            };
        }
    }
}
