using Oraculum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace OraculumCLI
{
    [Cmdlet(VerbsCommon.Push, "Fact")]
    public class PushFact : OraculumPSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public Fact? Fact { get; set; }

        protected override void ProcessRecord()
        {
            if (Fact == null)
            {
                WriteObject(false);
                return;
            }
            var j = Connection.UpdateFact(Fact);
            j.Wait();
            WriteObject(true);
        }
    }
}
