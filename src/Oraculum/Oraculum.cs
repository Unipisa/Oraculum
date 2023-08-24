using Newtonsoft.Json;
using OpenAI;
using OpenAI.Managers;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;
using WeaviateNET;

namespace Oraculum;

public class Configuration
{
    public string? WeaviateEndpoint { get; set; }
    public string? WeaviateApiKey { get; set; }
    public string? OpenAIApiKey { get; set; }
    public string? OpenAIOrgId { get; set; }
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

public class Fact
{
    internal const int MajorVersion = 1;
    internal const int MinorVersion = 0;
    public const string ClassName = "Facts";

    [JsonIgnore]
    public Guid? id;

    public string? factType;
    public string? category;
    public string[]? tags;
    public string? title;
    public string? content;
    public string? citation;
    public string? reference;
    public DateTime expiration;
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

    public Oraculum(Configuration conf) { 
        _configuration = conf;
        if (conf == null || conf.WeaviateEndpoint == null || conf.OpenAIApiKey == null)
            throw new ArgumentNullException(nameof(conf));
        _kb = new WeaviateDB(conf.WeaviateEndpoint, conf.WeaviateApiKey);
        _gpt = new OpenAIService(new OpenAiOptions() { ApiKey = conf.OpenAIApiKey, Organization = conf.OpenAIOrgId });
        _facts = null;
    }

    [MemberNotNull(nameof(_kb), nameof(_facts))]
    private void ensureConnection()
    {
        if (_kb == null || _facts == null) throw new Exception("Knowledge base not connected");
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
        await _kb.Schema.Update();
        if (!_kb.Schema.Classes.Where(c => c.Name == Fact.ClassName).Any() || !_kb.Schema.Classes.Where(c => c.Name == OraculumConfig.ClassName).Any())
        {
            throw new Exception($"Weaviate instance has not been configured for Oraculum, you must initialize the DB first");
        }

        _oraculumConfigClass = _kb.Schema.GetClass<OraculumConfig>(OraculumConfig.ClassName);
        if (_oraculumConfigClass == null)
            throw new Exception("Internal error: cannot get OraculumConfig class!");
        _oraculumConfig = await _oraculumConfigClass.Get(OraculumConfig.ConfigID);
        _facts = _kb.Schema.GetClass<Fact>(Fact.ClassName);
    }

    public async Task Init()
    {
        await _kb.Schema.Update();
        if (_kb.Schema.Classes.Where(c => c.Name == OraculumConfig.ClassName).Any())
        {
            _oraculumConfigClass = _kb.Schema.GetClass<OraculumConfig>(OraculumConfig.ClassName);
            if (_oraculumConfigClass == null)
                throw new Exception("Internal error: cannot get OraculumConfig class!");
            await _oraculumConfigClass.Delete();
            await _kb.Schema.Update();
        }

        if (!_kb.Schema.Classes.Where(c => c.Name == OraculumConfig.ClassName).Any())
        {
            _oraculumConfigClass = await _kb.Schema.NewClass<OraculumConfig>(OraculumConfig.ClassName);
            _oraculumConfig = _oraculumConfigClass.Create();
            _oraculumConfig.Id = OraculumConfig.ConfigID;
            _oraculumConfig.Properties.creationDate = DateTime.Now;
            _oraculumConfig.Properties.majorVersion = MajorVersion;
            _oraculumConfig.Properties.minorVersion = MinorVersion;
            _oraculumConfig.Properties.schemaMajorVersion = Fact.MajorVersion;
            _oraculumConfig.Properties.schemaMinorVersion = Fact.MinorVersion;
            await _oraculumConfigClass.Add(_oraculumConfig);
        }

        if (_kb.Schema.Classes.Where(c => c.Name == Fact.ClassName).Any())
        {
            _facts = _kb.Schema.GetClass<Fact>(Fact.ClassName);
            if (_facts == null)
                throw new Exception("Internal error: cannot get Facts class!"); 
            await _facts.Delete();
            await _kb.Schema.Update();
        }
        _facts = await _kb.Schema.NewClass<Fact>(Fact.ClassName);
    }

    public async Task<int> TotalFacts()
    {
        ensureConnection();

        return await _facts.CountObjects();
    }

    public async Task<int> TotalFactsByCategory(string category)
    {
        ensureConnection();

        return await _facts.CountObjectsByProperty(nameof(Fact.category), category);
    }

    public async Task AddFact(Fact fact)
    {
        ensureConnection();

        var f = _facts.Create();
        f.Properties = fact;

        await _facts.Add(f);
        fact.id = f.Id; // Assumption: Guid is generated by Create()
    }

    public async Task<int> AddFact(ICollection<Fact> facts)
    {
        ensureConnection();
        var toadd = new List<WeaviateObject<Fact>>();
        foreach (var fact in facts) {
            var f = _facts.Create();
            f.Properties = fact;
            toadd.Add(f);
        }
        var ret = await _facts.Add(toadd);
        return ret.Count;
    }

    public async Task UpgradeDB()
    {
        await _kb.Schema.Update();
        var sibyllaConfigClass = _kb.Schema.GetClass<OraculumConfig>("SibyllaConfig");
        if (sibyllaConfigClass != null)
        {
            await sibyllaConfigClass.Delete();
        }
    }
}
