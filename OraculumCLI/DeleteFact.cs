using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace OraculumCLI
{
    [Cmdlet(VerbsCommon.Remove, "Fact")]
    public class DeleteFact : OraculumPSCmdlet
    {
        [Parameter(Mandatory = true)]
        public Guid? Id { get; set; }

        protected override void ProcessRecord()
        {
            if (Id == null)
            {
                WriteObject(false);
                return;
            }
            var j = Connection.DeleteFact(Id.Value);
            j.Wait();
            WriteObject(j.Result);
        }

    }
}
