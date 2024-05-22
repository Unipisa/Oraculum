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
using WeaviateNET;
using OraculumApi.Models.FrontOffice;

namespace OraculumApi.Models
{
    [IndexNullState]
    public partial class EvaluateResponseDTO
    {
        public required string question { get; set;}
        public string? ground_truth { get; set;}

    }
}

