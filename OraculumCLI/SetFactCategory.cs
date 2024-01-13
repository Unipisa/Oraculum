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
    [Cmdlet(VerbsCommon.Set, "FactCategory")]
    public class SetFactCategory : OraculumPSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
        public Fact? Fact { get; set; }

        [Parameter(Mandatory = true)]
        public string? Category { get; set; }


        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            if (Fact == null)
            {
                WriteObject(false);
                return;
            }
            Fact.category = Category;
            var j = Connection.UpdateFact(Fact);
            j.Wait();

            WriteObject(Fact);
        }
    }
}
