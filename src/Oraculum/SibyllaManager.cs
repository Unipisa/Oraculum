using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Oraculum
{
    /// <summary>
    /// This class is used to host multiple Sibylla instances.
    /// </summary>
    public class SibyllaManager
    {
        private Dictionary<(string, Guid), (Sibylla, DateTime?)> _sibyllae = new Dictionary<(string, Guid), (Sibylla, DateTime?)>();
        private Dictionary<string, List<string>> response = new Dictionary<string, List<string>>();
        private List<string> completed = new List<string>();
        private string _dataDir;
        private Configuration _oraculumConf;
        private Oraculum _oraculum;

        public SibyllaManager(Configuration oraculumConf, string configurationsPath)
        {
            if (!Directory.Exists(configurationsPath))
                throw new DirectoryNotFoundException($"Configuration directory {configurationsPath} not found.");

            _dataDir = configurationsPath;
            _oraculumConf = oraculumConf;
            _oraculum = new Oraculum(oraculumConf);
        }

        private string ConfFile(string name)
        {
            var path = Path.Combine(_dataDir, $"{name}.json");
            if (!File.Exists(path))
                throw new FileNotFoundException($"Configuration file {path} not found.");

            return path;
        }

        public int Count => _sibyllae.Count;

        public List<string> AvailableConfigurations
        {
            get
            {
                var files = Directory.GetFiles(_dataDir, "*.json");
                var confs = new List<string>();
                foreach (var file in files)
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    confs.Add(name);
                }
                return confs;
            }
        }

        IEnumerable<(Guid, Sibylla)> GetActiveSibyllae(string name)
        {
            foreach (var key in _sibyllae.Keys)
            {
                var (n, id) = key;
                if (n == name)
                {
                    var (s, e) = _sibyllae[key];
                    if (e.HasValue && e.Value > DateTime.Now)

                        yield return (id, s);
                }
            }
        }


        public async Task<(Guid, Sibylla)> AddSibylla(string name, Configuration? oraculumConf = null, DateTime? expiration = null, bool connect = true)
        {
            var conf = SibyllaConf.FromJson(File.ReadAllText(ConfFile(name))) ?? throw new Exception($"Configuration file {ConfFile(name)} is not valid.");
            var s = new Sibylla(oraculumConf ?? _oraculumConf, conf);
            var id = Guid.NewGuid();
            _sibyllae.Add((name, id), (s, expiration));
            if (connect)
                await s.Connect();
            return (id, s);
        }

        public void CollectExpiredSibyllae()
        {
            var toremove = new List<(string, Guid)>();
            foreach (var key in _sibyllae.Keys)
            {
                var (_, expiration) = _sibyllae[key];
                if (expiration.HasValue && expiration.Value < DateTime.Now)
                    toremove.Add(key);
            }

            foreach (var key in toremove)
                _sibyllae.Remove(key);
        }

        public void RemoveSibylla(string name, Guid id)
        {
            _sibyllae.Remove((name, id));
        }

        public async Task<Fact?> GetFactById(Guid id)
        {
            // check Oraculum connection
            if (!_oraculum.IsConnected)
                await _oraculum.Connect();
            //try catch keynotfound, return null
            try
            {
                Fact? fact = await _oraculum.GetFact(id) ?? throw new KeyNotFoundException($"Fact with id {id} not found.");
                return fact;
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        //delete fact by id
        public async Task DeleteFactById(Guid id)
        {
            // check Oraculum connection
            if (!_oraculum.IsConnected)
                await _oraculum.Connect();
            //try catch keynotfound, return null
            try
            {
                await _oraculum.DeleteFact(id);
            }
            catch (KeyNotFoundException)
            {
                return;
            }
        }

        // find relevant facts
        public async Task<ICollection<Fact>> FindRelevantFacts(string query, float? distance = null, int? limit = null, int? autoCut = null, string[]? factTypeFilter = null, string[]? categoryFilter = null, string[]? tagsFilter = null)
        {
            // check Oraculum connection
            if (!_oraculum.IsConnected)
                await _oraculum.Connect();
            if (string.IsNullOrEmpty(query))
            {
                return new List<Fact>();
            }

            var factFilter = new FactFilter()
            {
                Limit = limit,
                Distance = distance,
                Autocut = autoCut,
                FactTypeFilter = factTypeFilter,
                CategoryFilter = categoryFilter,
                TagsFilter = tagsFilter
            };

            var facts = await _oraculum.FindRelevantFacts(query, null);

            return facts;
        }

        // Get all facts
        public async Task<ICollection<Fact>> GetAllFacts(int limit = 0, int offset = 0, string? sort = null, string? order = null)
        {
            // check Oraculum connection
            if (!_oraculum.IsConnected)
                await _oraculum.Connect();

            var facts = await _oraculum.ListFacts(limit: limit, offset: offset, sort: sort, order: order);

            return facts;
        }

        // Add List of facts
        public async Task<int> AddFacts(ICollection<Fact> facts)
        {
            // check Oraculum connection
            if (!_oraculum.IsConnected)
                await _oraculum.Connect();

            var newFacts = await _oraculum.AddFact(facts);

            return newFacts;
        }

        // get all Sibyllae Configurations
        public Task<List<SibyllaConf>> GetSibillae()
        {
            var sibyllaeConfigs = new List<SibyllaConf>();
            DirectoryInfo di = new DirectoryInfo(_dataDir);
            FileInfo[] confFiles = di.GetFiles("*.json");
            if (confFiles.Length != 0)
            {
                foreach (FileInfo fi in confFiles)
                {
                    var name = Path.GetFileNameWithoutExtension(fi.Name);
                    try
                    {
                        sibyllaeConfigs.Add(SibyllaConf.FromJson(File.ReadAllText(ConfFile(name))) ?? throw new Exception($"Configuration file {ConfFile(name)} is not valid."));
                    }
                    catch (System.Text.Json.JsonException)
                    {
                        return null;
                    }
                }
            }
            return Task.FromResult(sibyllaeConfigs);
        }

        public Sibylla GetSibylla(string name, Guid id)
        {
            if (!_sibyllae.ContainsKey((name, id)))
                throw new KeyNotFoundException($"Sibylla {name} with id {id} not found.");
            return _sibyllae[(name, id)].Item1;
        }

        public void ExtendExpiration(string name, Guid id, TimeSpan extension)
        {
            if (!_sibyllae.ContainsKey((name, id)))
                throw new KeyNotFoundException($"Sibylla {name} with id {id} not found.");
            var (s, e) = _sibyllae[(name, id)];
            _sibyllae[(name, id)] = (s, e + extension);
        }

        public void SetExpiration(string name, Guid id, DateTime expiration)
        {
            if (!_sibyllae.ContainsKey((name, id)))
                throw new KeyNotFoundException($"Sibylla {name} with id {id} not found.");
            var (s, _) = _sibyllae[(name, id)];
            _sibyllae[(name, id)] = (s, expiration);
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
