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

namespace OraculumApi.Models.FrontOffice
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public partial class ChatDetail
    {
        /// <summary>
        /// Gets or Sets Id
        /// </summary>
        [Required]

        [DataMember(Name = "id")]
        public required string Id { get; set; }

        /// <summary>
        /// Gets or Sets Name
        /// </summary>

        [DataMember(Name = "name")]
        public required string Name { get; set; }

        /// <summary>
        /// Gets or Sets SibyllaId
        /// </summary>

        [DataMember(Name = "sibyllaId")]
        public required string SibyllaId { get; set; }

        /// <summary>
        /// Gets or Sets ReferenceIds
        /// </summary>

        [DataMember(Name = "referenceIds")]
        public required ReferenceIds ReferenceIds { get; set; }

        /// <summary>
        /// Gets or Sets Messages
        /// </summary>

        [DataMember(Name = "messages")]
        public required List<Message> Messages { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class ChatDetail {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  SibyllaId: ").Append(SibyllaId).Append("\n");
            sb.Append("  ReferenceIds: ").Append(ReferenceIds).Append("\n");
            sb.Append("  Messages: ").Append(Messages).Append("\n");
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
