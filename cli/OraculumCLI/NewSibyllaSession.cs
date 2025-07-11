using Oraculum;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace OraculumCLI
{
    [Cmdlet(VerbsCommon.New, "SibyllaSession")]
    public class NewSibyllaSession : OraculumPSCmdlet
    {
        [Parameter]
        public SibyllaConf? Config { get; set; }

        [Parameter]
        public string? ConfigFile { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            if (ConfigFile != null)
            {
                var path = SessionState.Path.GetUnresolvedProviderPathFromPSPath(ConfigFile);
                var json = System.IO.File.ReadAllText(path);
                Config = System.Text.Json.JsonSerializer.Deserialize<SibyllaConf>(json);
            } else if (Config == null)
            {
                throw new Exception("Either ConfigFile or Config must be set");
            }

            var sibylla = new Sibylla(Connection.Configuration, Config!);
            sibylla.Connect().Wait();
            foreach (var m in sibylla.History)
            {
                if (m.Role == "assistant")
                    this.CommandRuntime.Host.UI.WriteLine(ConsoleColor.Yellow, ConsoleColor.Black, $"Assistant: {m.Content}");
                else if (m.Role == "user")
                    this.CommandRuntime.Host.UI.WriteLine($"You: {m.Content}");
            }
            do
            {
                this.CommandRuntime.Host.UI.Write("You: ");
                var user = this.CommandRuntime.Host.UI.ReadLine();
                if (user == "#quit")
                    break;
                this.CommandRuntime.Host.UI.Write(ConsoleColor.Yellow, ConsoleColor.Black, $"Assistant: ");
                var ena = sibylla.AnswerAsync(user);
                var en = ena.GetAsyncEnumerator();
                while(true)
                {
                    var j = en.MoveNextAsync();
                    j.AsTask().Wait();
                    if (!j.Result)
                        break;
                    this.CommandRuntime.Host.UI.Write(ConsoleColor.Yellow, ConsoleColor.Black, $"{en.Current}");
                }
                this.CommandRuntime.Host.UI.WriteLine();
            } while (true);
        }
    }
}
