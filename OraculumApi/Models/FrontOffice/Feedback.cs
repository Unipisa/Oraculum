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
    public partial class Feedback
    {
        /// <summary>
        /// Gets or Sets RefId
        /// </summary>

        [DataMember(Name = "refId")]
        public string RefId { get; set; }

        /// <summary>
        /// Gets or Sets Text
        /// </summary>

        [DataMember(Name = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or Sets Rating
        /// </summary>

        [DataMember(Name = "rating")]
        public string Rating { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class Feedback {\n");
            sb.Append("  RefId: ").Append(RefId).Append("\n");
            sb.Append("  Text: ").Append(Text).Append("\n");
            sb.Append("  Rating: ").Append(Rating).Append("\n");
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
