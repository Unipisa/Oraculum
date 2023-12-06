using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oraculum
{
    public class CheckAndAnswer : IFunction
    {
        private Sibylla sibylla;

        public CheckAndAnswer(Sibylla sibylla)
        {
            this.sibylla = sibylla;
        }

        public object Execute(Dictionary<string, object> args)
        {
            // Call the private CheckAndAnswerFunction here
            return CheckAndAnswerFunction(args);
        }

        private object CheckAndAnswerFunction(Dictionary<string, object> args)
        {
            // Your existing logic here
            if (args.TryGetValue("evaluation", out var evaluation))
            {
                string? evaluationString = evaluation?.ToString();

                if (evaluationString != null && evaluationString.Equals("False", StringComparison.OrdinalIgnoreCase))
                {
                    sibylla.MarkLastHistoryMessageAsOT();
                }

                return evaluation!;
            }
            else
            {
                throw new ArgumentException("Argument 'evaluation' is missing.");
            }
        }
    }
}
