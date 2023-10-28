using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI;
using OpenAI.Extensions;
using OpenAI.Managers;
using OpenAI.ObjectModels;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.Json.Nodes;
using WeaviateNET;
using WeaviateNET.Query;
using WeaviateNET.Query.AdditionalProperty;
using WeaviateNET.Query.ConditionalOperator;

namespace Oraculum;

public class Configuration
{
    public static Configuration FromJson(string json)
    {
        return System.Text.Json.JsonSerializer.Deserialize<Configuration>(json)!;
    }

    internal OpenAIService CreateService()
    {
        if (Provider == ProviderType.OpenAi)
            return new OpenAIService(new OpenAiOptions() { 
                ProviderType = ProviderType.OpenAi,
                ApiKey = OpenAIApiKey!, 
                Organization = OpenAIOrgId 
            });
        else if (Provider == ProviderType.Azure)
            return new OpenAIService(new OpenAiOptions() {
                ProviderType = ProviderType.Azure,
                ApiKey = AzureOpenAIApiKey!,
                ResourceName = AzureResourceName!,
                DeploymentId = AzureDeploymentId!
            });
        else
            throw new Exception($"Unknown provider {Provider}");
    }

    internal bool IsValid()
    {
        if (WeaviateEndpoint == null || WeaviateApiKey == null)
            return false;
        if (Provider == ProviderType.OpenAi)
            return OpenAIApiKey != null && OpenAIOrgId != null;
        else if (Provider == ProviderType.Azure)
            return AzureOpenAIApiKey != null && AzureResourceName != null && AzureDeploymentId != null;
        else
            return false;
    }

    public string? WeaviateEndpoint { get; set; }
    public string? WeaviateApiKey { get; set; }
    public OpenAI.ProviderType Provider { get; set; } = ProviderType.OpenAi;
    public string? OpenAIApiKey { get; set; }
    public string? OpenAIOrgId { get; set; }
    public string? AzureDeploymentId { get; set; }
    public string? AzureResourceName { get; set; }
    public string? AzureOpenAIApiKey { get; set; }
}

public class OraculumConfig
{
    public const string ClassName = "OraculumConfig";
    public static readonly Guid ConfigID = Guid.Parse("afc7b344-6c26-4b72-9fbb-0df1acf26c5a");

    public int majorVersion;
    public int minorVersion;
    public DateTime creationDate;
    public int schemaMajorVersion;
    public int schemaMinorVersion;
}

public class FactFilter
{
    public int? Limit = null;
    public float? Distance = null;
    public int? Autocut = null;
    public string[]? FactTypeFilter = null;
    public string[]? CategoryFilter = null;
    public string[]? TagsFilter = null;
}

[IndexNullState]
public class Fact_1_0
{
    internal const int MajorVersion = 1;
    internal const int MinorVersion = 0;

    [JsonIgnore]
    public const string ClassName = "Facts";

    [JsonIgnore]
    public Guid? id;
    [JsonIgnore]
    public double? distance;

    public string? factType;
    public string? category;
    public string[]? tags;
    public string? title;
    public string? content;
    public string? citation;
    public string? reference;
    public DateTime? expiration;
}

[IndexNullState]
public class Fact
{
    internal const int MajorVersion = 1;
    internal const int MinorVersion = 1;

    [JsonIgnore]
    public const string ClassName = "Facts";

    [JsonIgnore]
    public Guid? id;
    [JsonIgnore]
    public double? distance;

    public string? factType;
    public string? category;
    public string[]? tags;
    public string? title;
    public string? content;
    public string? citation;
    public string? reference;
    public DateTime? expiration;
    public GeoCoordinates? location;
    public double? locationDistance;
    public string? locationName;
    public string[]? editPrincipals;
    public string? validFrom;
    public string? validTo;
    //public WeaviateRef[]? references;
    public DateTime? factAdded;
}

    public class Oraculum
{
    internal const int MajorVersion = 1;
    internal const int MinorVersion = 0;

    private Configuration _configuration;
    private WeaviateDB _kb;
    private OpenAIService _gpt;
    private WeaviateClass<Fact>? _facts;
    private WeaviateClass<OraculumConfig>? _oraculumConfigClass;
    private WeaviateObject<OraculumConfig>? _oraculumConfig;
    private ILogger _logger;

    public Configuration Configuration
    {
        get
        {
            return _configuration;
        }
    }

    public Oraculum(Configuration conf, ILogger? logger = null) {
        _logger = logger ?? NullLogger.Instance;
        _configuration = conf;
        if (!conf.IsValid())
        {
            _logger.Log(LogLevel.Critical, "Oraculum: configuration not provided");
            throw new ArgumentNullException(nameof(conf));
        }
        _kb = new WeaviateDB(conf.WeaviateEndpoint, conf.WeaviateApiKey);
        _gpt = conf.CreateService();
        _facts = null;
    }

    [MemberNotNull(nameof(_kb), nameof(_facts))]
    private void ensureConnection()
    {
        if (_kb == null || _facts == null)
            throw new Exception("Knowledge base not connected");
    }

    public async Task<bool> IsKBInitialized()
    {
        // Todo: add schema check
        await _kb.Schema.Update();
        return _kb.Schema.Classes.Where(c => c.Name == Fact.ClassName).Any() && _kb.Schema.Classes.Where(c => c.Name == OraculumConfig.ClassName).Any();
    }

    public bool IsConnected
    {
        get
        {
            return _kb != null && _facts != null;
        }
    }

    public async Task Connect()
    {
        _logger.Log(LogLevel.Trace, "Connect: Loading Weaviate schema and checking whether contains OraculumConfig and Facts classes");
        await _kb.Schema.Update();
        if (!_kb.Schema.Classes.Where(c => c.Name == Fact.ClassName).Any() || !_kb.Schema.Classes.Where(c => c.Name == OraculumConfig.ClassName).Any())
        {
            throw new Exception($"Weaviate instance has not been configured for Oraculum, you must initialize the DB first");
        }

        await ReadSchemaMetadata();

        if (_oraculumConfig!.Properties.schemaMajorVersion != Fact.MajorVersion || _oraculumConfig.Properties.schemaMinorVersion != Fact.MinorVersion)
        {
            _logger.Log(LogLevel.Warning, $"Connect: Weaviate schema version is {_oraculumConfig.Properties.schemaMajorVersion}.{_oraculumConfig.Properties.schemaMinorVersion} while Oraculum requires {Fact.MajorVersion}.{Fact.MinorVersion}. Trying to update schema...");
            await UpgradeDB();
        }

        _facts = _kb.Schema.GetClass<Fact>(Fact.ClassName);
    }

    private async Task ReadSchemaMetadata()
    {
        _oraculumConfigClass = _kb.Schema.GetClass<OraculumConfig>(OraculumConfig.ClassName);
        if (_oraculumConfigClass == null)
            throw new Exception("Internal error: cannot get OraculumConfig class!");
        _oraculumConfig = await _oraculumConfigClass.Get(OraculumConfig.ConfigID);

        if (_oraculumConfig == null)
            throw new Exception("Internal error: cannot get OraculumConfig object!");
    }

    public async Task Init()
    {
        _logger.Log(LogLevel.Information, "Init DB Schema");
        await _kb.Schema.Update();
        if (_kb.Schema.Classes.Where(c => c.Name == OraculumConfig.ClassName).Any())
        {
            _oraculumConfigClass = _kb.Schema.GetClass<OraculumConfig>(OraculumConfig.ClassName);
            if (_oraculumConfigClass == null)
            {
                _logger.Log(LogLevel.Critical, "Init: cannot get OraculumConfig class, aborting initialization");
                throw new Exception("Internal error: cannot get OraculumConfig class!");
            }
            await _oraculumConfigClass.Delete();
            await _kb.Schema.Update();
        }

        if (!_kb.Schema.Classes.Where(c => c.Name == OraculumConfig.ClassName).Any())
        {
            _logger.Log(LogLevel.Trace, "Init: creating OraculumConfig class");
            _oraculumConfigClass = await _kb.Schema.NewClass<OraculumConfig>(OraculumConfig.ClassName);
            _oraculumConfig = _oraculumConfigClass.Create();
            _oraculumConfig.Id = OraculumConfig.ConfigID;
            _oraculumConfig.Properties.creationDate = DateTime.Now;
            _oraculumConfig.Properties.majorVersion = MajorVersion;
            _oraculumConfig.Properties.minorVersion = MinorVersion;
            _oraculumConfig.Properties.schemaMajorVersion = Fact.MajorVersion;
            _oraculumConfig.Properties.schemaMinorVersion = Fact.MinorVersion;
            await _oraculumConfigClass.Add(_oraculumConfig);
            _logger.Log(LogLevel.Trace, "Init: Saved oraculum configuration");
        }

        if (_kb.Schema.Classes.Where(c => c.Name == Fact.ClassName).Any())
        {
            _logger.Log(LogLevel.Trace, "Init: fetching Facts class for deletion");
            _facts = _kb.Schema.GetClass<Fact>(Fact.ClassName);
            if (_facts == null)
            {
                _logger.Log(LogLevel.Critical, "Init: error accessing Facts class");
                throw new Exception("Internal error: cannot get Facts class!");
            }
            _logger.Log(LogLevel.Trace, "Init: deleting Facts class");
            await _facts.Delete();
            await _kb.Schema.Update();
        }
        _logger.Log(LogLevel.Trace, "Init: creating Facts class");
        _facts = await _kb.Schema.NewClass<Fact>(Fact.ClassName);
        await _kb.Schema.Update();
        _facts.Properties.Where(p => p.Name == nameof(Fact.expiration)).First().IndexSearchable = true;
        await _facts.Update();
    }

    public async Task<int> TotalFacts()
    {
        ensureConnection();
        _logger.Log(LogLevel.Trace, "TotalFacts: counting facts");
        return await _facts.CountObjects();
    }

    public async Task<int> TotalFactsByCategory(string category)
    {
        ensureConnection();
        _logger.Log(LogLevel.Trace, $"TotalFactsByCategory: counting facts by category '{category}'");
        return await _facts.CountObjectsByProperty(nameof(Fact.category), category);
    }

    public async Task<Guid?> AddFact(Fact fact)
    {
        ensureConnection();
        _logger.Log(LogLevel.Information, "AddFact: adding fact");
        var f = _facts.Create();
        f.Properties = fact;

        await _facts.Add(f);
        fact.id = f.Id; // Assumption: Guid is generated by Create()
        return f.Id;
    }

    public async Task<int> AddFact(ICollection<Fact> facts)
    {
        ensureConnection();
        _logger.Log(LogLevel.Information, "AddFact: adding a collection of facts");
        var toadd = new List<WeaviateObject<Fact>>();
        foreach (var fact in facts) {
            var f = _facts.Create();
            f.Properties = fact;
            toadd.Add(f);
        }
        var ret = await _facts.Add(toadd);
        return ret.Count;
    }

    public async Task<Fact?> GetFact(Guid id)
    {
        ensureConnection();
        _logger.Log(LogLevel.Trace, $"GetFact: fetching fact {id}");
        var fact = await _facts.Get(id);
        if (fact == null) return null;
        fact.Properties.id = fact.Id;
        return fact.Properties;
    }

    public async Task<bool> DeleteFact(Guid id)
    {
        ensureConnection();
        _logger.Log(LogLevel.Information, $"DeleteFact: deleting fact {id}");
        var fact = await _facts.Get(id);
        if (fact == null) return false;

        await fact.Delete();
        return true;
    }

    public async Task UpgradeDB()
    {
        await _kb.Schema.Update();

        // If upgradeDB is invoked alone, without a previous call to Connect(), we need to read the schema metadata
        if (_oraculumConfigClass == null)
        {
            await ReadSchemaMetadata();
        }

        if (_oraculumConfig!.Properties.schemaMajorVersion == 1 && _oraculumConfig!.Properties.schemaMinorVersion == 0)
        {
            var facts = _kb.Schema.GetClass<Fact_1_0>(Fact_1_0.ClassName);
            if (facts == null)
            {
                _logger.Log(LogLevel.Critical, "UpgradeDB: cannot get Facts class");
                throw new Exception("Internal error: cannot get Facts class!");
            }
            var fn = Path.GetTempFileName();
            var outf = new StreamWriter(fn);

            var n = await facts.CountObjects();
            for (var i = 0; i < n; i += 100)
            {
                var r = await facts.ListObjects(100, offset: i);
                var outt = JsonConvert.SerializeObject(r.Objects.ToList());
                outf.WriteLine(outt.Length);
                await outf.WriteLineAsync(outt);
            }
            outf.Close();

            await facts.Delete();
            var factsNew = await _kb.Schema.NewClass<Fact>(Fact.ClassName);

            _oraculumConfig!.Properties.schemaMajorVersion = Fact.MajorVersion;
            _oraculumConfig!.Properties.schemaMinorVersion = Fact.MinorVersion;
            await _oraculumConfig!.Save();

            var inf = new StreamReader(fn);

            string? line;
            var total = 0;
            while ((line = inf.ReadLine()) != null)
            {
                var sz = int.Parse(line);
                var buf = new char[sz];
                await inf.ReadBlockAsync(buf, 0, sz);
                var facts_1_0 = JsonConvert.DeserializeObject<List<WeaviateObject<Fact_1_0>>>(new string(buf));
                var facts_1_1 = facts_1_0!.ConvertAll(f =>
                {
                    var o = factsNew.Create();
                    o.Properties.category = f.Properties.category;
                    o.Properties.citation = f.Properties.citation;
                    o.Properties.content = f.Properties.content;
                    o.Properties.expiration = f.Properties.expiration;
                    o.Properties.factType = f.Properties.factType;
                    o.Properties.reference = f.Properties.reference;
                    o.Properties.tags = f.Properties.tags;
                    o.Properties.title = f.Properties.title;
                    return o;
                });
                var na = await factsNew.Add(facts_1_1);
                total += na.Count;
                if (na.Count != facts_1_1.Count)
                {
                    _logger.Log(LogLevel.Critical, $"UpgradeDB: error adding facts, expected {facts_1_1.Count} but added {na.Count}");
                }
                inf.ReadLine();
            }
            if (total != n)
            {
                _logger.Log(LogLevel.Critical, $"UpgradeDB: error adding facts, expected {n} but added {total}");
            }

        }
        //var facts = _kb.Schema.GetClass<Fact>(Fact.ClassName);
        //var r = await facts.ListObjects(100);
        //var bk = r.Objects.ToList();
        //await facts.Delete();
        //facts = await _kb.Schema.NewClass<Fact>(Fact.ClassName);
        //await facts.Add(bk);
    }

    public async Task<ICollection<Fact>> ListFacts(long limit=1024, long offset=0, string? sort=null, string? order=null)
    {
        ensureConnection();

        _logger.Log(LogLevel.Trace, $"ListFacts: listing facts with limit {limit}, offset {offset}, sort {sort}, order {order}");
        var facts = await _facts.ListObjects(limit,offset: offset, sort: sort, order: order);
        if (facts == null) return new List<Fact>();
        var ret = new List<Fact>();
        foreach (var fact in facts.Objects)
        {
            fact.Properties.id = fact.Id;
            ret.Add(fact.Properties);
        }
        return ret;
    }

    public async Task<ICollection<Fact>> FindRelevantFacts(string concept, FactFilter? factFilter = null)
    {
        ensureConnection();
        _logger.Log(LogLevel.Information, $"FindRelevantFacts: finding facts relevant to '{concept}'");
        if (factFilter == null)
            factFilter = new FactFilter();
        _logger.Log(LogLevel.Trace, $"FindRelevantFacts: finding facts relevant to '{concept}' with Filter '{JsonConvert.SerializeObject(factFilter)}'");

        var qg = _facts.CreateGetQuery(selectall: true);
        qg.Filter.NearText(concept, distance: factFilter.Distance);
        if (factFilter.Limit.HasValue) qg.Filter.Limit(factFilter.Limit.Value);
        if (factFilter.Autocut.HasValue) qg.Filter.Autocut(factFilter.Autocut.Value);
        var andcond = new List<ConditionalAtom<Fact>>() { 
            Conditional<Fact>.Or(
                When<Fact, DateTime>.GreaterThanEqual(nameof(Fact.expiration), DateTime.Now),
                When<Fact, DateTime>.IsNull(nameof(Fact.expiration))
            )};
        if (factFilter.FactTypeFilter != null)
            andcond.Add(When<Fact, string[]>.ContainsAny(nameof(Fact.factType), factFilter.FactTypeFilter));
        if (factFilter.CategoryFilter != null)
            andcond.Add(When<Fact, string[]>.ContainsAny(nameof(Fact.category), factFilter.CategoryFilter));
        if (factFilter.TagsFilter != null)
            andcond.Add(When<Fact, string[]>.ContainsAny(nameof(Fact.tags), factFilter.TagsFilter));
        qg.Filter.Where(Conditional<Fact>.And(andcond.ToArray()));
        qg.Fields.Additional.Add(Additional.Id, Additional.Distance);
        var query = new GraphQLQuery();
        query.Query = qg.ToString();

        _logger.Log(LogLevel.Trace, $"FindRelevantFacts: query to Weaviate {query.Query}");

        var res = await _kb.Schema.RawQuery(query);
        if (res.Errors != null && res.Errors.Count > 0)
        {
            throw new Exception($"Error querying Weaviate");
        }
        _logger.Log(LogLevel.Trace, $"FindRelevantFacts: Query result {res.Data["Get"].ToString()}");
        var ret = new List<Fact>();
#pragma warning disable CS8602 // Disable warning for derefernce potentially null value since I know it should be ok.
        foreach (var f in res.Data["Get"]["Facts"])
        {
            Guid? id = f["_additional"]["id"] == null ? null : Guid.Parse(f["_additional"]["id"].ToString());
            double? dist = f["_additional"]["distance"] == null ? null : f["_additional"]["distance"].ToObject<double>();
            var o = f.ToObject<Fact>();
            o.id = id;
            o.distance = dist;
            ret.Add(o);
        }
#pragma warning restore CS8602

        return ret;
    }
}
