using Oraculum;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Text;
using System.Threading.Tasks;
using WeaviateNET;

namespace OraculumCLI
{
    [Cmdlet(VerbsData.Restore, "FactsDB")]
    public class RestoreFactsDB : OraculumPSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string? FileName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            var otp = Random.Shared.Next(100000, 999999);
            var ret = this.CommandRuntime.Host.UI.Prompt("Restore Facts DB", $"To restore the backup type in the code {otp}", new Collection<FieldDescription>() { new FieldDescription("otp") { IsMandatory = true, Label = "OTP code" } });
            if (ret["otp"].ToString() != otp.ToString())
            {
                WriteObject(false);
                return;
            }

            var progress = new ProgressRecord(otp, "restoring", "Number of facts restored");
            var j = Connection.RestoreFacts(FileName!, (num, total) => { 
                progress.PercentComplete = (int)(num /(double)total); 
                progress.StatusDescription = $"Written {num} of {total} facts";
                return 0;
            });
            while (j.Status == TaskStatus.Running)
            {
                Thread.Sleep(100);
                WriteProgress(progress);
            }
            j.Wait();
            WriteObject(j.Result);
        }
    }
}
