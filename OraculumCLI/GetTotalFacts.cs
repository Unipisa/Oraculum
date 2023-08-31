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
    public class GetTotalFacts : OraculumPSCmdlet
    {
        protected override void ProcessRecord()
        {
            var j = Connection.TotalFacts();
            j.Wait();
            WriteObject(j.Result);
        }
    }
}
