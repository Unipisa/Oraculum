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

public interface IPersistentFactProvider
{
    public Task<int> TotalFacts();
    public Task<int> TotalFactsByCategory(string category);
    public Task<Guid?> AddFact(Fact fact);
    public Task<int> AddFact(ICollection<Fact> facts);
    public Task<Fact?> GetFact(Guid id);
    public Task<bool> DeleteFact(Guid id);
    public Task UpdateFact(Fact fact);
    public Task UpgradeDB();
    public Task<int> BackupFacts(string fn, Func<int, int, int>? progress = null);
    public Task<int> RestoreFacts(string fn, Func<int, int, int>? progress = null);
    public Task<ICollection<Fact>> ListFacts(long limit = 1024, long offset = 0, string? sort = null, string? order = null);
    public Task<ICollection<Fact>> ListFilteredFacts(FactFilter factFilter);
}

public interface IPersistentGenericObjectProvider
{
    public Task<Guid?> AddGenericObject(GenericObject genericObject);
    public Task<GenericObject?> GetGenericObject(Guid id);
    public Task<ICollection<GenericObject>> ListGenericObjects(long limit = 1024, long offset = 0, string? sort = null, string? order = null);
    public Task UpdateGenericObject(GenericObject genericObject);
    public Task<bool> DeleteGenericObject(Guid id);
}

public interface IPersistentKnowledgeProvider : IFactProvider, IPersistentFactProvider
{
    public Task<bool> IsKBInitialized();
    public Task Init();
    public Task Connect();
    public bool IsConnected { get; }
    public void Disconnect();
}