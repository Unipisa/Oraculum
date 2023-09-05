using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using Oraculum;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace OraculumInteractive
{
    public class OraculumKernel : Kernel, IKernelCommandHandler<SubmitCode>
    {
        private readonly Configuration oraculumConfig;
        private readonly SibyllaConf sibyllaConf;
        private readonly Command useCategoryFilterCommand = new Command("#!use-category-filter");
        private readonly Command noCategoryFilterCommand = new Command("#!no-category-filter");
        private readonly Argument<string[]> useCategoryFilterArgument = new Argument<string[]>();
        private string[]? categoryFilterOverride = null;

        private Sibylla? sibylla;

        private async Task<Sibylla> ConnectSibyllaAsync()
        {
            if (sibylla == null)
            {
                sibylla = new Sibylla(oraculumConfig, sibyllaConf);
                await sibylla.Connect();
            }
            return sibylla;
        }   

        public OraculumKernel(string name, Oraculum.Configuration oraculumConfig, SibyllaConf sibyllaConf) : base(name)
        {
            this.oraculumConfig = oraculumConfig;
            this.sibyllaConf = sibyllaConf;
            useCategoryFilterCommand.AddArgument(useCategoryFilterArgument);
            useCategoryFilterCommand.SetHandler(context => {
                var categorFilter = context.ParseResult.GetValueForArgument(useCategoryFilterArgument);
                categoryFilterOverride = categorFilter;
            });
            noCategoryFilterCommand.SetHandler(context =>
            {
                categoryFilterOverride = null;
            });
            AddDirective(useCategoryFilterCommand);
            AddDirective(noCategoryFilterCommand);
        }

        async Task IKernelCommandHandler<SubmitCode>.HandleAsync(SubmitCode command, KernelInvocationContext context)
        {
            var client = await ConnectSibyllaAsync();
            var answer = await client.Answer(command.Code, categoryFilter: categoryFilterOverride);
            var formattedValues = FormattedValue.CreateManyFromObject(answer);
            var returnValueProduced = new ReturnValueProduced(answer, command, formattedValues);
            context.Publish(returnValueProduced);
        }
    }
}