using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace OraculumApi.Models.BackOffice
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public partial class SearchCriteria
    {
        /// <summary>
        /// Search query to find relevant facts.
        /// </summary>
        /// <value>Search query to find relevant facts.</value>
        [Required]

        [DataMember(Name = "query")]
        public required string Query { get; set; }

        /// <summary>
        /// Distance criterion for search.
        /// </summary>
        /// <value>Distance criterion for search.</value>

        [DataMember(Name = "distance")]
        public float? Distance { get; set; }

        /// <summary>
        /// Limit the number of facts returned.
        /// </summary>
        /// <value>Limit the number of facts returned.</value>

        [DataMember(Name = "limit")]
        public int? Limit { get; set; }

        /// <summary>
        /// Auto-cut criterion for search.
        /// </summary>
        /// <value>Auto-cut criterion for search.</value>

        [DataMember(Name = "autoCut")]
        public int? AutoCut { get; set; }

        /// <summary>
        /// Filter facts by type.
        /// </summary>
        /// <value>Filter facts by type.</value>

        [DataMember(Name = "factTypeFilter")]
        public string[]? FactTypeFilter { get; set; }

        /// <summary>
        /// Filter facts by category.
        /// </summary>
        /// <value>Filter facts by category.</value>

        [DataMember(Name = "categoryFilter")]
        public string[]? CategoryFilter { get; set; }

        [DataMember(Name = "autoCutPercentage")]
        public float? AutoCutPercentage { get; set; }

        /// <summary>
        /// Filter facts by tags.
        /// </summary>
        /// <value>Filter facts by tags.</value>

        [DataMember(Name = "tagsFilter")]
        public string[]? TagsFilter { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class SearchCriteria {\n");
            sb.Append("  Query: ").Append(Query).Append("\n");
            sb.Append("  Distance: ").Append(Distance).Append("\n");
            sb.Append("  Limit: ").Append(Limit).Append("\n");
            sb.Append("  AutoCut: ").Append(AutoCut).Append("\n");
            sb.Append("  FactTypeFilter: ").Append(FactTypeFilter).Append("\n");
            sb.Append("  CategoryFilter: ").Append(CategoryFilter).Append("\n");
            sb.Append("  TagsFilter: ").Append(TagsFilter).Append("\n");
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
    }
}
