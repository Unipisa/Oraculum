using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oraculum
{
    public interface IFunction
    {
        object Execute(Dictionary<string, object> args);
    }
}
