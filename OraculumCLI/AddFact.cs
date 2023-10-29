using Oraculum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using WeaviateNET;

namespace OraculumCLI
{
    [Cmdlet(VerbsCommon.Add, "Fact")]
    public class AddFact : OraculumPSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string? FactType { get; set; }

        [Parameter]
        public string? Category { get; set; }

        [Parameter]
        public string[]? Tags { get; set; }

        [Parameter]
        public string? Title { get; set; }

        [Parameter(Mandatory = true)]
        public string? Content { get; set; }

        [Parameter]
        public string? Citation { get; set; }

        [Parameter]
        public string? Reference { get; set; }

        [Parameter]
        public DateTime? Expiration { get; set; }

        [Parameter]
        public DateTime? FactAdded { get; set; }

        [Parameter]
        public GeoCoordinates? Location { get; set; }

        [Parameter]
        public string? LocationName { get; set; }

        [Parameter]
        public double? LocationDistance { get; set; }

        [Parameter]
        public string[]? EditPrincipals { get; set; }

        [Parameter]
        public string? ValidFrom { get; set; }

        [Parameter]
        public string? ValidTo { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            var fact = new Fact()
            {
                factType = FactType,
                category = Category,
                tags = Tags,
                title = Title,
                content = Content,
                citation = Citation,
                reference = Reference,
                expiration = Expiration,
                location = Location,
                locationName = LocationName,
                locationDistance = LocationDistance,
                editPrincipals = EditPrincipals,
                validFrom = ValidFrom,
                validTo = ValidTo,
                factAdded = FactAdded ?? DateTime.Now
            };
            Connection.AddFact(fact).Wait();
        }
    }
}
