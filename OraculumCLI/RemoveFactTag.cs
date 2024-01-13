using Oraculum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using WeaviateNET;

namespace OraculumCLI
{
    [Cmdlet(VerbsCommon.Remove, "FactTag")]
    public class RemoveFactTag : OraculumPSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
        public Fact? Fact { get; set; }

        [Parameter(Mandatory = true)]
        public string? Tag { get; set; }


        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            if (Fact == null)
            {
                WriteObject(false);
                return;
            }
            if (Fact.tags != null && Fact.tags.Contains(Tag))
            {
                var tags = new List<string>(Fact.tags);
                tags.Remove(Tag!);
                if (tags.Count == 0)
                {
                    Fact.tags = null;
                }
                else
                {
                    Fact.tags = tags.ToArray();
                }
                var j = Connection.UpdateFact(Fact);
                j.Wait();
            }
            WriteObject(Fact);
        }
    }
}
