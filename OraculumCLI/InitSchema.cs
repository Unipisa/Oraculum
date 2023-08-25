using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using WeaviateNET;

namespace OraculumCLI
{
    [Cmdlet(VerbsCommon.Reset, "Schema")]
    public class InitSchema : Cmdlet
    {
        [Parameter(Mandatory = true)]
        public Oraculum.Oraculum? Oraculum { get; set; }
        protected override void ProcessRecord()
        {
            if (Oraculum == null)
            {
                WriteObject(false);
                return;
            }

            var otp = Random.Shared.Next(100000, 999999);
            var ret = this.CommandRuntime.Host.UI.Prompt("Schema reset", $"To reset the schema type in the code {otp}", new Collection<FieldDescription>() { new FieldDescription("otp") { IsMandatory = true, Label = "OTP code" } });
            if (ret["otp"].ToString() != otp.ToString())
            {
                WriteObject(false);
                return;
            }
            var j = Oraculum.Init();
            j.Wait();
            WriteObject(true);
        }
    }
}
