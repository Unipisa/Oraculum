using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace OraculumApi.Models.FrontOffice
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public partial class ChatSummary
    {
        /// <summary>
        /// Gets or Sets Id
        /// </summary>
        [Required]

        [DataMember(Name = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or Sets Name
        /// </summary>
        [Required]

        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets SibyllaId
        /// </summary>

        [DataMember(Name = "sibyllaId")]
        public string SibyllaId { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class ChatSummary {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  SibyllaId: ").Append(SibyllaId).Append("\n");
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
