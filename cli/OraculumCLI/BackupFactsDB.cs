using Oraculum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using WeaviateNET;
using static System.Net.WebRequestMethods;

namespace OraculumCLI
{
    [Cmdlet(VerbsData.Backup, "FactsDB")]
    public class BackupFactsDB : OraculumPSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string? FileName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            var progress = new ProgressRecord(1, "restoring", "Number of facts restored");
            var path = SessionState.Path.GetUnresolvedProviderPathFromPSPath(FileName!);
            var j = Connection.BackupFacts(path, (num, total) => {
                progress.PercentComplete = (int)(num / (double)total);
                progress.StatusDescription = $"Read {num} of {total} facts";
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
