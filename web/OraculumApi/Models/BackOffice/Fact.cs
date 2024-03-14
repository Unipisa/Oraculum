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
        public Guid? Id { get; set; }

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
        public List<string>? Tags { get; set; }

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
        public string? Citation { get; set; }

        /// <summary>
        /// Reference for the fact
        /// </summary>
        /// <value>Reference for the fact</value>

        [DataMember(Name = "reference")]
        public string? Reference { get; set; }

        /// <summary>
        /// Expiration date of the fact
        /// </summary>
        /// <value>Expiration date of the fact</value>

        [DataMember(Name = "expiration")]
        public DateTime? Expiration { get; set; }
        public bool? OutOfLimit { get; internal set; }
        public double? Distance { get; private set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            return $@"
class Fact {{
    Id: {Id}
    FactType: {FactType}
    Category: {Category}
    Tags: {Tags}
    Title: {Title}
    Content: {Content}
    Citation: {Citation}
    Reference: {Reference}
    Expiration: {Expiration}
    OutOfLimit: {OutOfLimit}
    Distance: {Distance}
}}
";
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
                Expiration = oraculumFact.expiration,
                Distance = oraculumFact.distance ?? 0,
            };
        }
    }
}
