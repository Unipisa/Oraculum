using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace OraculumCLI
{
    [Cmdlet(VerbsCommon.Find, "RelevantFacts")]
    public class FindRelevantFacts : OraculumPSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string? Query { get; set; }

        [Parameter]
        public float? Distance { get; set; } = null;

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
            if (Query == null)
            {
                WriteObject(false);
                return;
            }
            var facts = Connection.FindRelevantFacts(Query, new Oraculum.FactFilter() 
            {
                Limit = Limit,
                Distance = Distance,
                Autocut = AutoCut,
                FactTypeFilter = FactTypeFilter,
                CategoryFilter = CategoryFilter,
                TagsFilter = TagsFilter
            });

            facts.Wait();
            
            foreach (var fact in facts.Result)
            {
                WriteObject(fact);
            }
        }
    }
}
