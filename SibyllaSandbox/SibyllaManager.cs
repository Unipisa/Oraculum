using Oraculum;

namespace SibyllaSandbox
{
    public class SibyllaManager
    {
        private Dictionary<string, Sibylla> _sibyllae = new Dictionary<string, Sibylla>();
        private Dictionary<string, List<string>> response = new Dictionary<string, List<string>>();
        private List<string> completed = new List<string>();

        public Dictionary<string, Sibylla> Sibyllae
        {
            get
            {
                return _sibyllae;
            }
        }

        public Dictionary<string, List<string>> Response
        {
            get
            {
                return response;
            }
        }

        public List<string> Completed
        {
            get
            {
                return completed;
            }
        }

    }
}
