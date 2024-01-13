using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace OraculumCLI
{
    [Cmdlet(VerbsCommon.Get, "Fact")]
    public class GetFact : OraculumPSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
        public Guid? Id { get; set; }

        protected override void ProcessRecord()
        {
            if (Id == null)
            {
                WriteObject(false);
                return;
            }
            var j = Connection.GetFact(Id.Value);
            j.Wait();
            WriteObject(j.Result);
        }
    }
}
