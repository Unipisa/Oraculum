using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace OraculumCLI
{
    [Cmdlet(VerbsCommon.Get, "Fact")]
    public class GetFact : Cmdlet
    {
        [Parameter(Mandatory = true)]
        public Oraculum.Oraculum? Oraculum { get; set; }

        [Parameter(Mandatory = true)]
        public Guid? Id { get; set; }

        protected override void ProcessRecord()
        {
            if (Oraculum == null || Id == null)
            {
                WriteObject(false);
                return;
            }
            var j = Oraculum.GetFact(Id.Value);
            j.Wait();
            WriteObject(j.Result);
        }
    }
}
