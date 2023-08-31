using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI;
using OpenAI.Extensions;
using OpenAI.Managers;
using OpenAI.ObjectModels;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection.Metadata;
using System.Text.Json.Nodes;
using WeaviateNET;
using WeaviateNET.Query;
using WeaviateNET.Query.AdditionalProperty;
using WeaviateNET.Query.ConditionalOperator;

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

[IndexNullState]
public class Fact
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
        await _kb.Schema.Update();
        _facts.Properties.Where(p => p.Name == nameof(Fact.expiration)).First().IndexSearchable = true;
        await _facts.Update();
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

    public async Task<Guid?> AddFact(Fact fact)
    {
        ensureConnection();

        var f = _facts.Create();
        f.Properties = fact;

        await _facts.Add(f);
        fact.id = f.Id; // Assumption: Guid is generated by Create()
        return f.Id;
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

    public async Task<Fact?> GetFact(Guid id)
    {
        ensureConnection();

        var fact = await _facts.Get(id);
        if (fact == null) return null;
        fact.Properties.id = fact.Id;
        return fact.Properties;
    }

    public async Task<bool> DeleteFact(Guid id)
    {
        ensureConnection();
        var fact = await _facts.Get(id);
        if (fact == null) return false;

        await fact.Delete();
        return true;
    }

    public async Task UpgradeDB()
    {
        //await _kb.Schema.Update();

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

    public async Task<ICollection<Fact>> FindRelevantFacts(string concept, int? limit = null, float? distance = null, int? autocut = null, string[]? factTypeFilter = null, string[]? categoryFilter = null, string[]? tagsFilter = null)
    {
        ensureConnection();

        var qg = _facts.CreateGetQuery(selectall: true);
        qg.Filter.NearText(concept, distance: distance);
        if (limit.HasValue) qg.Filter.Limit(limit.Value);
        if (autocut.HasValue) qg.Filter.Autocut(autocut.Value);
        var andcond = new List<ConditionalAtom<Fact>>() { 
            Conditional<Fact>.Or(
                When<Fact, DateTime>.GreaterThanEqual(nameof(Fact.expiration), DateTime.Now),
                When<Fact, DateTime>.IsNull(nameof(Fact.expiration))
            )};
        if (factTypeFilter != null)
            andcond.Add(When<Fact, string[]>.ContainsAny(nameof(Fact.factType), factTypeFilter));
        if (categoryFilter != null)
            andcond.Add(When<Fact, string[]>.ContainsAny(nameof(Fact.category), categoryFilter));
        if (tagsFilter != null)
            andcond.Add(When<Fact, string[]>.ContainsAny(nameof(Fact.tags), tagsFilter));
        qg.Filter.Where(Conditional<Fact>.And(andcond.ToArray()));
        qg.Fields.Additional.Add(Additional.Id, Additional.Distance);
        var query = new GraphQLQuery();
        query.Query = qg.ToString();
        #region handmade query
        var goal = $@"{{
    Get{{
      {Fact.ClassName} (
        nearText: {{
          concepts: [""{concept}""]{(distance.HasValue ? $",\n          distance: {distance.Value.ToString(CultureInfo.InvariantCulture)}\n" : "")}
        }}{(limit.HasValue ? $"\n        limit: {limit}\n" : "")}{(autocut.HasValue ? $"\n        autocut: {autocut}\n" : "")}
        where: {{
            operator: And,
            operands: [
             {{
              operator: Or,
              operands: [
                {{
                  operator: LessThanEqual,
                  path: [""expiration""],
                  valueDate: {JsonConvert.SerializeObject(DateTime.Now)}
                 }}
                 {{
                   operator: IsNull,
                   valueBoolean: true,
                   path: [""expiration""]
                 }}
              ]
             }}{(factTypeFilter != null ? $@",
             {{
                operator: ContainsAny,
                path: [""factType""],
                valueText: {JsonConvert.SerializeObject(factTypeFilter)}
             }}" : "")}{(categoryFilter != null ? $@",
             {{
                operator: ContainsAny,
                path: [""category""],
                valueText: {JsonConvert.SerializeObject(categoryFilter)}
             }}" : "")}{(tagsFilter != null ? $@",
             {{
                operator: ContainsAny,
                path: [""tags""],
                valueText: {JsonConvert.SerializeObject(tagsFilter)}
             }}" : "")}
            ]
        }}
      ){{
        title
        content
        factType
        category
        tags
        citation
        reference
        expiration
        _additional {{
           id
           distance
        }}
      }}
    }}
}}";
        #endregion
        var res = await _kb.Schema.RawQuery(query);
        if (res.Errors != null && res.Errors.Count > 0)
        {
            throw new Exception($"Error querying Weaviate");
        }
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
