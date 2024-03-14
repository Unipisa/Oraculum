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
    public class InitSchema : OraculumPSCmdlet
    {
        protected override void ProcessRecord()
        {
            var otp = Random.Shared.Next(100000, 999999);
            var ret = this.CommandRuntime.Host.UI.Prompt("Schema reset", $"To reset the schema type in the code {otp}", new Collection<FieldDescription>() { new FieldDescription("otp") { IsMandatory = true, Label = "OTP code" } });
            if (ret["otp"].ToString() != otp.ToString())
            {
                WriteObject(false);
                return;
            }
            Connection.Init().Wait();
            WriteObject(true);
        }
    }
}
