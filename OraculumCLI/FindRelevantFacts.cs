using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace OraculumCLI
{
    [Cmdlet(VerbsCommon.Find, "RelevantFacts")]
    public class FindRelevantFacts : Cmdlet
    {
        [Parameter(Mandatory = true)]
        public Oraculum.Oraculum? Oraculum { get; set; }

        [Parameter(Mandatory = true)]
        public string? Query { get; set; }

        [Parameter]
        public double? Distance { get; set; } = null;

        [Parameter]
        public int? Limit { get; set; } = null;

        [Parameter]
        public int? AutoCut { get; set; } = null;

        [Parameter]
        public string[]? FactTypeFilter { get; set; } = null;

        [Parameter]
        public string[]? CategoryFilter { get; set; } = null;

        [Parameter]
        public string[]? TagsFilter { get; set; } = null;

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            if (Oraculum == null || Query == null)
            {
                WriteObject(false);
                return;
            }
            var facts = Oraculum.FindRelevantFacts(Query, Limit, Distance, AutoCut, FactTypeFilter, CategoryFilter, TagsFilter);

            facts.Wait();
            
            foreach (var fact in facts.Result)
            {
                WriteObject(fact);
            }
        }
    }
}
