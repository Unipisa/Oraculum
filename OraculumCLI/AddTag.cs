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
    [Cmdlet(VerbsCommon.Add, "Tag")]
    public class AddTag : OraculumPSCmdlet
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
            if (Fact.tags == null || !Fact.tags.Contains(Tag))
            {
                var tags = Fact.tags == null ? new List<string>() : new List<string>(Fact.tags);
                tags.Add(Tag!);
                Fact.tags = tags.ToArray();
                var j = Connection.UpdateFact(Fact);
                j.Wait();
            }
            WriteObject(Fact);
        }
    }
}
