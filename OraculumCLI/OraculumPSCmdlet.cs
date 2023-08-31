using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace OraculumCLI
{
    public class OraculumPSCmdlet : PSCmdlet
    {
        [Parameter]
        public Oraculum.Oraculum? Oraculum { get; set; }

        public Oraculum.Oraculum Connection
        {
            get
            {
                var oraculum = Oraculum != null ? Oraculum! : (Oraculum.Oraculum)SessionState.PSVariable.Get("Oraculum").Value;
                if (oraculum == null)
                {
                    WriteError(new ErrorRecord(new Exception("Oraculum not initialized"), "OraculumNotInitialized", ErrorCategory.InvalidOperation, null));
                    throw new Exception("Oraculum not initialized");
                }
                return oraculum;
            }
        }
    }
}
