using Oraculum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

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
                expiration = Expiration
            };
            Connection.AddFact(fact).Wait();
        }
    }
}
