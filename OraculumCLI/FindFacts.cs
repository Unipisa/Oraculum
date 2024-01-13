using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace OraculumCLI
{
    [Cmdlet(VerbsCommon.Find, "Facts")]
    public class FindFacts : OraculumPSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string? Query { get; set; }

        [Parameter]
        public int? Limit { get; set; } = null;

        [Parameter]
        public string[]? FactTypeFilter { get; set; } = null;

        [Parameter]
        public string[]? CategoryFilter { get; set; } = null;

        [Parameter]
        public string[]? TagsFilter { get; set; } = null;

        [Parameter]
        public DateTime? AddedSince { get; set; } = null;

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            if (Query == null)
            {
                WriteObject(false);
                return;
            }
            var facts = Connection.ListFilteredFacts(new Oraculum.FactFilter() 
            {
                Limit = Limit,
                FactTypeFilter = FactTypeFilter,
                CategoryFilter = CategoryFilter,
                TagsFilter = TagsFilter,
                AddedSince = AddedSince
            });

            facts.Wait();
            
            foreach (var fact in facts.Result)
            {
                WriteObject(fact);
            }
        }
    }
}
