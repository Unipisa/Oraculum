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
            if (args.TryGetValue("valutazione", out var valutazione))
            {
                string? valutazioneString = valutazione?.ToString();

                if (valutazioneString != null && valutazioneString.Equals("False", StringComparison.OrdinalIgnoreCase))
                {
                    sibylla.MarkLastHistoryMessageAsOT();
                }

                return valutazione!;
            }
            else
            {
                throw new ArgumentException("Argument 'valutazione' is missing.");
            }
        }
    }
}
