using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using WeaviateNET;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using WeaviateNET.Query.AdditionalProperty;
using WeaviateNET.Query.ConditionalOperator;
using WeaviateNET.Query;

namespace Oraculum.Providers.Facts
{
    public class WeaviateOptions
    {
        [Required]
        [Url]
        public string? BaseUrl { get; set; }
        public string? ApiKey { get; set; }
        public string? UserName { get; set; }
    }

    public class Weaviate : IPersistentKnowledgeProvider, IPersistentGenericObjectProvider
    {
        internal const int MajorVersion = 1;
        internal const int MinorVersion = 0;

        private WeaviateOptions _configuration;
        private WeaviateDB _kb;
        private WeaviateClass<Fact>? _facts;
        private WeaviateClass<OraculumConfig>? _oraculumConfigClass;
        private WeaviateObject<OraculumConfig>? _oraculumConfig;
        private WeaviateClass<GenericObject>? _genericObjectClass;

        private ILogger _logger;

        public Weaviate(WeaviateOptions configuration, ILogger? logger = null)
        {
            _configuration = configuration;
            _logger = logger ?? NullLogger.Instance;
            _kb = new WeaviateDB(_configuration.BaseUrl!, _configuration.ApiKey);
        }

        public Weaviate(IConfiguration configuration, ILogger? logger = null)
        {
            _configuration = new WeaviateOptions();
            configuration.Bind(_configuration);
            _logger = logger ?? NullLogger.Instance;
            _kb = new WeaviateDB(_configuration.BaseUrl!, _configuration.ApiKey);
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
            if (_configuration.UserName != null)
            {
                _logger.Log(LogLevel.Trace, $"Connect: Default user name is set to {_configuration.UserName}");
            }
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

        public void Disconnect()
        {
            _facts = null;
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
            if (_configuration.UserName != null)
            {
                if (fact.editPrincipals == null)
                    fact.editPrincipals = new string[] { $"O:{_configuration.UserName}" };
                else
                    fact.editPrincipals = fact.editPrincipals.Append(_configuration.UserName).ToArray();
            }
            var f = _facts.Create();
            f.Properties = fact;

            await _facts.Add(f);
            fact.id = f.Id; // Assumption: Guid is generated by Create()
            return f.Id;
        }

        public async Task UpdateFact(Fact fact)
        {
            ensureConnection();

            var obj = await _facts.Get(fact.id!.Value);
            if (obj == null)
                throw new Exception($"Cannot find fact {fact.id}");
            obj.Properties = fact;
            await obj.Save();
        }

        public async Task<int> AddFact(ICollection<Fact> facts)
        {
            ensureConnection();
            _logger.Log(LogLevel.Information, "AddFact: adding a collection of facts");
            var toadd = new List<WeaviateObject<Fact>>();
            foreach (var fact in facts)
            {
                if (_configuration.UserName != null)
                {
                    if (fact.editPrincipals == null)
                        fact.editPrincipals = new string[] { $"O:{_configuration.UserName}" };
                    else
                        fact.editPrincipals = fact.editPrincipals.Append(_configuration.UserName).ToArray();
                }
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
            else if (_oraculumConfig!.Properties.schemaMajorVersion == 1 && _oraculumConfig!.Properties.schemaMinorVersion == 1)
            {
                _logger.Log(LogLevel.Information, "UpgradeDB: upgrading from 1.1 to 1.2");
                var facts = _kb.Schema.GetClass<Fact_1_1>(Fact_1_1.ClassName);
                if (facts == null)
                {
                    _logger.Log(LogLevel.Critical, "UpgradeDB: cannot get Facts class");
                    throw new Exception("Internal error: cannot get Facts class!");
                }
                var fn = Path.GetTempFileName();
                var outf = new StreamWriter(fn);

                _logger.Log(LogLevel.Trace, $"UpgradeDB: backing up facts into {fn}");

                var n = await facts.CountObjects();
                for (var i = 0; i < n; i += 100)
                {
                    var r = await facts.ListObjects(100, offset: i);
                    var outt = JsonConvert.SerializeObject(r.Objects.ToList());
                    outf.WriteLine(outt.Length);
                    await outf.WriteLineAsync(outt);
                }
                outf.Close();

                _logger.Log(LogLevel.Trace, $"UpgradeDB: exported {n} facts");

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
                    _logger.Log(LogLevel.Trace, $"UpgradeDB: Written {total} facts");
                    var sz = int.Parse(line);
                    var buf = new char[sz];
                    await inf.ReadBlockAsync(buf, 0, sz);
                    var facts_1_1 = JsonConvert.DeserializeObject<List<WeaviateObject<Fact_1_1>>>(new string(buf));
                    var facts_1_2 = facts_1_1!.ConvertAll(f =>
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
                        o.Properties.location = f.Properties.location;
                        o.Properties.locationDistance = f.Properties.locationDistance;
                        o.Properties.locationName = f.Properties.locationName;
                        o.Properties.editPrincipals = f.Properties.editPrincipals;
                        o.Properties.validFrom = f.Properties.validFrom;
                        o.Properties.validTo = f.Properties.validTo;
                        o.Properties.factAdded = f.Properties.factAdded;

                        return o;
                    });
                    var na = await factsNew.Add(facts_1_2);
                    total += na.Count;
                    if (na.Count != facts_1_2.Count)
                    {
                        _logger.Log(LogLevel.Critical, $"UpgradeDB: error adding facts, expected {facts_1_2.Count} but added {na.Count}");
                    }
                    inf.ReadLine();
                }
                if (total != n)
                {
                    _logger.Log(LogLevel.Critical, $"UpgradeDB: error adding facts, expected {n} but added {total}");
                }

            }

        }

        public async Task<int> BackupFacts(string fn, Func<int, int, int>? progress = null)
        {
            var facts = _kb.Schema.GetClass<Fact>(Fact.ClassName);
            var outf = new StreamWriter(fn);
            var n = await facts!.CountObjects();
            outf.WriteLine(n);
            for (var i = 0; i < n; i += 100)
            {
                var r = await facts.ListObjects(100, offset: i);
                var outt = JsonConvert.SerializeObject(r.Objects.ToList());
                outf.WriteLine(outt.Length);
                await outf.WriteLineAsync(outt);
                progress?.Invoke(i, n);
            }
            outf.Close();
            return n;
        }

        public async Task<int> RestoreFacts(string fn, Func<int, int, int>? progress = null)
        {
            var facts = _kb.Schema.GetClass<Fact>(Fact.ClassName);
            await facts!.Delete();
            facts = await _kb.Schema.NewClass<Fact>(Fact.ClassName);

            var inf = new StreamReader(fn);

            string? line;
            line = inf.ReadLine();
            var totalcount = int.Parse(line!);
            var total = 0;
            while ((line = inf.ReadLine()) != null)
            {
                var sz = int.Parse(line);
                var buf = new char[sz];
                await inf.ReadBlockAsync(buf, 0, sz);
                var data = JsonConvert.DeserializeObject<List<WeaviateObject<Fact>>>(new string(buf));
                var datacount = data!.Count;
                var na = await facts!.Add(data!);
                total += na.Count;
                datacount -= na.Count;
                var ids = na.Select(f => f.Id).ToList();
                var errors = (from d in data where (!ids.Contains(d!.Id)) select d).ToList();
                while (errors.Count > 0)
                {
                    na = await facts.Add(errors);
                    total += na.Count;
                    datacount -= na.Count;
                    ids = na.Select(f => f.Id).ToList();
                    errors = (from d in errors where (!ids.Contains(d!.Id)) select d).ToList();
                }
                if (datacount > 0)
                {
                    _logger.Log(LogLevel.Critical, $"Error adding facts, expected {data.Count} but added {na.Count}");
                }
                inf.ReadLine();
                progress?.Invoke(total, totalcount);
            }
            return total;
        }

        public async Task<ICollection<Fact>> ListFacts(long limit = 1024, long offset = 0, string? sort = null, string? order = null)
        {
            ensureConnection();

            _logger.Log(LogLevel.Trace, $"ListFacts: listing facts with limit {limit}, offset {offset}, sort {sort}, order {order}");
            var facts = await _facts.ListObjects(limit, offset: offset, sort: sort, order: order);
            if (facts == null) return new List<Fact>();
            var ret = new List<Fact>();
            foreach (var fact in facts.Objects)
            {
                fact.Properties.id = fact.Id;
                ret.Add(fact.Properties);
            }
            return ret;
        }

        public async Task<ICollection<Fact>> ListFilteredFacts(FactFilter factFilter)
        {
            ensureConnection();
            _logger.Log(LogLevel.Information, $"ListFilteredFacts: Filter '{JsonConvert.SerializeObject(factFilter)}'");

            var qg = _facts.CreateGetQuery(selectall: true);
            if (factFilter.Limit.HasValue) qg.Filter.Limit(factFilter.Limit.Value);
            if (factFilter.Autocut.HasValue)
                _logger.Log(LogLevel.Warning, $"ListFilteredFacts: Autocut is ignored in ListFilteredFacts");
            if (factFilter.AutocutPercentage.HasValue)
                _logger.Log(LogLevel.Warning, $"ListFilteredFacts: AutocutPercentage is ignored in ListFilteredFacts");
            var andcond = new List<ConditionalAtom<Fact>>() {
            Conditional<Fact>.Or(
                When<Fact, DateTime>.GreaterThanEqual(nameof(Fact.expiration), DateTime.Now),
                When<Fact, DateTime>.IsNull(nameof(Fact.expiration))
            )};
            if (factFilter.AddedSince.HasValue)
                andcond.Add(When<Fact, DateTime>.GreaterThanEqual(nameof(Fact.factAdded), factFilter.AddedSince.Value));
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
                var o = f.ToObject<Fact>();
                o.id = id;
                ret.Add(o);
            }
#pragma warning restore CS8602

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
            if (factFilter.AddedSince.HasValue)
                andcond.Add(When<Fact, DateTime>.GreaterThanEqual(nameof(Fact.factAdded), factFilter.AddedSince.Value));
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

            if (factFilter.AutocutPercentage.HasValue)
            {
                var min = ret.First().distance;
                var max = ret.Last().distance;
                var top = (from fact in ret
                           where (((fact.distance - min) / (max - min)) < factFilter.AutocutPercentage)
                           select fact).Count();
                ret = ret.Take(top).ToList();
            }

            return ret;
        }
        private async Task InitializeGenericObjectClass()
        {
            if (_genericObjectClass == null)
            {
                if (_kb.Schema.Classes.Where(c => c.Name == GenericObject.ClassName).Any())
                {
                    _genericObjectClass = _kb.Schema.GetClass<GenericObject>(GenericObject.ClassName);
                    _genericObjectClass!.Properties.Where(p => p.Name == nameof(GenericObject.content)).First().IndexSearchable = true;
                }
                else
                {
                    _genericObjectClass = await _kb.Schema.NewClass<GenericObject>(GenericObject.ClassName);
                    _genericObjectClass!.Properties.Where(p => p.Name == nameof(GenericObject.content)).First().IndexSearchable = true;
                    await _genericObjectClass.Update();
                }
            }

        }

        private async Task EnsureGenericObjectClassAsync()
        {
            if (_genericObjectClass == null)
                await InitializeGenericObjectClass();
        }

        public async Task<Guid?> AddGenericObject(GenericObject genericObject)
        {
            ensureConnection();
            await EnsureGenericObjectClassAsync();

            ensureConnection();
            _logger.Log(LogLevel.Information, "AddFact: adding GenricObject");
            var obj = _genericObjectClass!.Create();
            obj.Properties = genericObject;

            await _genericObjectClass.Add(obj);
            genericObject.id = obj.Id; // Assumption: Guid is generated by Create()
            return obj.Id;
        }

        public async Task<GenericObject?> GetGenericObject(Guid id)
        {
            ensureConnection();
            await EnsureGenericObjectClassAsync();

            var obj = await _genericObjectClass!.Get(id);
            return obj?.Properties;
        }

        public async Task<ICollection<GenericObject>> ListGenericObjects(long limit = 1024, long offset = 0, string? sort = null, string? order = null)
        {
            ensureConnection();
            await EnsureGenericObjectClassAsync();

            var objs = await _genericObjectClass!.ListObjects(limit, offset: offset, sort: sort, order: order);
            if (objs == null) return new List<GenericObject>();
            var ret = new List<GenericObject>();
            foreach (var obj in objs.Objects)
            {
                obj.Properties.id = obj.Id;
                ret.Add(obj.Properties);
            }
            return ret;
        }

        public async Task UpdateGenericObject(GenericObject genericObject)
        {
            ensureConnection();
            await EnsureGenericObjectClassAsync();

            var obj = await _genericObjectClass!.Get(genericObject.id!.Value);
            if (obj == null)
                throw new Exception($"Cannot find GenericObject with id {genericObject.id}");

            obj.Properties = genericObject;
            await obj.Save();
        }

        public async Task<bool> DeleteGenericObject(Guid id)
        {
            ensureConnection();
            await EnsureGenericObjectClassAsync();

            var obj = await _genericObjectClass!.Get(id);
            if (obj == null) return false;

            await obj.Delete();
            return true;
        }
    }
}
