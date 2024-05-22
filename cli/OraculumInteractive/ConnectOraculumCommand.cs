using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Connection;
using Oraculum;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaviateNET;

namespace OraculumInteractive
{
    public class ConnectOraculumCommand : ConnectKernelCommand
    {
        private Option<FileInfo> oraculumConfigFileOption = new Option<FileInfo>("--oraculum-config", "path to Oraculum configuration file") { IsRequired = true }.ExistingOnly();
        private Option<FileInfo> sibyllaConfigFileOption = new Option<FileInfo>("--sibylla-config", "path to Sibylla configuration file") { IsRequired = false }.ExistingOnly();
        private Option<string[]> categoryFilterOption = new Option<string[]>(new[] { "--category-filter", "-c" }, "category filter") { IsRequired = false };
        private Option<string> sibyllaConfigJsonOption = new Option<string>("--sibylla-config-json", "Sibylla configuration JSON") { IsRequired = false };

        public ConnectOraculumCommand() : base("Oraculum", "ninnipraje")
        {
            AddOption(oraculumConfigFileOption);
            AddOption(sibyllaConfigFileOption);
            AddOption(categoryFilterOption);
            AddOption(sibyllaConfigJsonOption);
        }

        public override Task<IEnumerable<Kernel>> ConnectKernelsAsync(KernelInvocationContext context, InvocationContext commandLineContext)
        {
            var kernelName = commandLineContext.ParseResult.GetValueForOption(KernelNameOption);
            var oraculumConfigFile = commandLineContext.ParseResult.GetValueForOption(oraculumConfigFileOption);
            var oraculumConfig = Oraculum.OraculumConfiguration.FromJson(System.IO.File.ReadAllText(oraculumConfigFile!.FullName));
            var sibyllaConfigJson = commandLineContext.ParseResult.GetValueForOption(sibyllaConfigJsonOption);
            if (string.IsNullOrEmpty(sibyllaConfigJson))
            {
                var sibyllaConfigFile = commandLineContext.ParseResult.GetValueForOption(sibyllaConfigFileOption);
                if (sibyllaConfigFile == null)
                {
                    throw new Exception("Either --sibylla-config or --sibylla-config-json must be specified");
                }
                sibyllaConfigJson = System.IO.File.ReadAllText(sibyllaConfigFile!.FullName);
            }
            var sibyllaConfig = SibyllaConf.FromJson(sibyllaConfigJson);
            var categoryFilter = commandLineContext.ParseResult.GetValueForOption(categoryFilterOption);
            if (categoryFilter?.Length > 0 )
            {
                var conf = sibyllaConfig.MemoryConfiguration;
                conf.CategoryFilter = categoryFilter;
                sibyllaConfig.MemoryConfiguration = conf;
            }
            var kernel = new OraculumKernel(kernelName!, oraculumConfig, sibyllaConfig);
            return Task.FromResult<IEnumerable<Kernel>>(new[] { kernel });
        }
    }
}
