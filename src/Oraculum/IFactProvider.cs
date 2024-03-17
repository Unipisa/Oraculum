using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oraculum;

public class FactFilter
{
    public int? Limit = null;
    public float? Distance = null;
    public int? Autocut = null;
    public float? AutocutPercentage = null;
    public string[]? FactTypeFilter = null;
    public string[]? CategoryFilter = null;
    public string[]? TagsFilter = null;
    public DateTime? AddedSince = null;
}

public interface IFactProvider
{
    public Task<ICollection<Fact>> FindRelevantFacts(string concept, FactFilter? factFilter = null);
}
