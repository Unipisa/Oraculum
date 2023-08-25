using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace OraculumCLI
{
    [Cmdlet("Test", "Schema")]
    public class CheckDBSchema : Cmdlet
    {
        [Parameter(Mandatory = true)]
        public Oraculum.Oraculum? Oraculum { get; set; }
        protected override void ProcessRecord()
        {
           base.ProcessRecord();
            if (Oraculum == null)
            {
                WriteObject(false);
                return;
            }
            var j = Oraculum.IsKBInitialized();
            j.Wait();
            WriteObject(j.Result);
        }
    }
}
