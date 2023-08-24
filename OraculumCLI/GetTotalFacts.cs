using Oraculum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace OraculumCLI
{
    [Cmdlet(VerbsCommon.Get, "TotalFact")]
    public class GetTotalFacts : Cmdlet
    {
        [Parameter(Mandatory = true)]
        public Oraculum.Oraculum Oraculum { get; set; } = null!;

        protected override void ProcessRecord()
        {
            var j = Oraculum.TotalFacts();
            j.Wait();
            WriteObject(j.Result);
        }
    }
}
