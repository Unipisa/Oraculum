using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace OraculumCLI
{
    [Cmdlet("Test", "Schema")]
    public class CheckDBSchema : OraculumPSCmdlet
    {
        protected override void ProcessRecord()
        {
           base.ProcessRecord();
            var j = Connection.IsKBInitialized();
            j.Wait();
            WriteObject(j.Result);
        }
    }
}
