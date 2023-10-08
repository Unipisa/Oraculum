using System;
using System.IO;

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

        public SibyllaManager(Configuration oraculumConf, string configurationsPath)
        {
            if (!Directory.Exists(configurationsPath))
                throw new DirectoryNotFoundException($"Configuration directory {configurationsPath} not found.");

            _dataDir = configurationsPath;
            _oraculumConf = oraculumConf;
        }

        private string ConfFile(string name)
        {
            var path = Path.Combine(_dataDir, $"{name}.json");
            if (!File.Exists(path))
                throw new FileNotFoundException($"Configuration file {path} not found.");

            return path;
        }

        public int Count => _sibyllae.Count;

        public List<string> AvailableConfigurations {
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
            var conf = SibyllaConf.FromJson(File.ReadAllText(ConfFile(name)));
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
