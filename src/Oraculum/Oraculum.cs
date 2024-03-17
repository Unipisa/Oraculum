using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Oraculum;

public class Oraculum
{
    private Configuration _configuration;
    private ILogger _logger;
    private IPersistentKnowledgeProvider _provider;
    private IPersistentGenericObjectProvider _genericObjectProvider;

    public Configuration Configuration
    {
        get
        {
            return _configuration;
        }
    }

    public ILogger Logger
    {
        get
        {
            return _logger;
        }
        set
        {
            _logger = value ?? NullLogger.Instance;
        }
    }

    public Oraculum(Configuration conf, ILogger? logger = null)
    {
        _logger = logger ?? NullLogger.Instance;
        _configuration = conf;
        if (!conf.IsValid())
        {
            _logger.Log(LogLevel.Critical, "Oraculum: configuration not provided");
            throw new ArgumentNullException(nameof(conf));
        }
        var provider = new Providers.Facts.Weaviate(new Providers.Facts.WeaviateOptions()
        {
            BaseUrl = conf.WeaviateEndpoint,
            ApiKey = conf.WeaviateApiKey,
            UserName = conf.UserName
        }, _logger);
        _provider = provider;
        _genericObjectProvider = provider;
    }

    public bool IsConnected
    {
        get
        {
            return _provider.IsConnected;
        }
    }

    public async Task Connect()
    {
        await _provider.Connect();
    }

    public async Task Init()
    {
        await _provider.Init();
    }

    public async Task<bool> IsKBInitialized()
    {
        return await _provider.IsKBInitialized();
    }

    public async Task<int> TotalFacts()
    {
        return await _provider.TotalFacts();
    }

    public async Task<int> TotalFactsByCategory(string category)
    {
        return await _provider.TotalFactsByCategory(category);
    }

    public async Task<Guid?> AddFact(Fact fact)
    {
        return await _provider.AddFact(fact);
    }

    public async Task UpdateFact(Fact fact)
    {
        await _provider.UpdateFact(fact);
    }

    public async Task<int> AddFact(ICollection<Fact> facts)
    {
        return await _provider.AddFact(facts);
    }

    public async Task<Fact?> GetFact(Guid id)
    {
        return await _provider.GetFact(id);
    }

    public async Task<bool> DeleteFact(Guid id)
    {
        return await _provider.DeleteFact(id);
    }

    public async Task UpgradeDB()
    {
        await _provider.UpgradeDB();
    }

    public async Task<int> BackupFacts(string fn, Func<int, int, int>? progress = null)
    {
        return await _provider.BackupFacts(fn, progress);
    }

    public async Task<int> RestoreFacts(string fn, Func<int, int, int>? progress = null)
    {
        return await _provider.RestoreFacts(fn, progress);
    }

    public async Task<ICollection<Fact>> ListFacts(long limit = 1024, long offset = 0, string? sort = null, string? order = null)
    {
        return await _provider.ListFacts(limit, offset, sort, order);
    }

    public async Task<ICollection<Fact>> ListFilteredFacts(FactFilter factFilter)
    {
        return await _provider.ListFilteredFacts(factFilter);
    }

    public async Task<ICollection<Fact>> FindRelevantFacts(string concept, FactFilter? factFilter = null)
    {
        return await _provider.FindRelevantFacts(concept, factFilter);
    }

    public async Task<Guid?> AddGenericObject(GenericObject genericObject)
    {
        return await _genericObjectProvider.AddGenericObject(genericObject);
    }

    public async Task<GenericObject?> GetGenericObject(Guid id)
    {
        return await _genericObjectProvider.GetGenericObject(id);
    }

    public async Task<ICollection<GenericObject>> ListGenericObjects(long limit = 1024, long offset = 0, string? sort = null, string? order = null)
    {
        return await _genericObjectProvider.ListGenericObjects(limit, offset, sort, order);
    }

    public async Task UpdateGenericObject(GenericObject genericObject)
    {
        await _genericObjectProvider.UpdateGenericObject(genericObject);
    }

    public async Task<bool> DeleteGenericObject(Guid id)
    {
        return await _genericObjectProvider.DeleteGenericObject(id);
    }
}
