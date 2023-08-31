using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace OraculumCLI
{
    [Cmdlet(VerbsCommon.Get, "Facts")]
    public class ListFacts : OraculumPSCmdlet
    {
        [Parameter]
        public long Limit { get; set; } = 1024;

        [Parameter]
        public long Offset { get; set; } = 0;

        [Parameter]
        public string? Sort { get; set; } = null;

        [Parameter]
        public string? Order { get; set; } = null;

        protected override void ProcessRecord()
        {
            var j = Connection.ListFacts(limit: Limit, offset: Offset, sort: Sort, order: Order);
            j.Wait();
            foreach (var o in j.Result)
            {
                WriteObject(o);
            }
        }
    }
}
