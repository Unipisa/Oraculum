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

namespace OraculumApi.Models.User
{
    /// <summary>
    /// Represents the user profile information, including roles.
    /// </summary>
    [DataContract]
    public partial class UserProfile
    {
        /// <summary>
        /// A list of roles associated with the user
        /// </summary>
        /// <example>["sysadmin", "backoffice", "frontoffice"]</example>
        [DataMember(Name = "roles")]
        public required List<String> Roles { get; set; }



        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            return $@"
class Fact {{
    Roles: {Roles}
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

    }
}
