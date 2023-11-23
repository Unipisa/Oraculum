using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using OpenAI.ObjectModels.RequestModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Oraculum
{
    internal class Memory
    {
        private Dictionary<Guid, (Fact, int, DateTime)> _memory;
        private List<ChatMessage> _history;
        private int _ttl;
        private int _span;
        private Oraculum _oraculum;
        private ILogger _logger;

        internal List<ChatMessage> History => _history;

        internal Oraculum Oraculum => _oraculum;

        internal Memory(Oraculum oraculum, int ttl = 5, int memorySpan = 4, ILogger? logger = null)
        {
            _logger = logger ?? NullLogger.Instance;
            _memory = new Dictionary<Guid, (Fact, int, DateTime)>();
            _history = new List<ChatMessage>();
            _ttl = ttl;
            _oraculum = oraculum;
            _span = memorySpan;
        }

        internal async Task<(XmlDocument, List<ChatMessage>)> Recall(string message, KnowledgeFilter? filter = null)
        {
            if (filter == null)
                filter = new KnowledgeFilter();

            _logger.Log(LogLevel.Trace, $"Recall: recall from memory related to '{message}' with knowledge filter {JsonConvert.SerializeObject(filter)}");

            _logger.Log(LogLevel.Trace, $"Recall: memory before {JsonConvert.SerializeObject(_memory)}");
            var toremove = new List<Guid>();
            foreach (var id in _memory.Keys)
            {
                var (fact, ttl, d) = _memory[id];
                ttl--;
                if (ttl <= 0)
                    toremove.Add(id);
                else
                    _memory[id] = (fact, ttl, d);
            }

            foreach (var id in toremove)
                _memory.Remove(id);

            _logger.Log(LogLevel.Trace, $"Recall: memory after cleanup {JsonConvert.SerializeObject(_memory)}");

            var facts = await _oraculum.FindRelevantFacts(message, new FactFilter()
            {
                FactTypeFilter = filter.FactTypeFilter,
                CategoryFilter = filter.CategoryFilter,
                TagsFilter = filter.TagsFilter,
                Limit = filter.Limit
            });

            var n = 0;
            foreach (var fact in facts)
            {
                if (!_memory.ContainsKey(fact.id!.Value))
                    _memory.Add(fact.id!.Value, (fact, Math.Max(1, _ttl - n), DateTime.Now));
                else
                {
                    var (f, ttl, d) = _memory[fact.id!.Value];
                    _memory[fact.id!.Value] = (fact, Math.Max(ttl + 1, _ttl - n), d);
                }
                ++n;
            }

            _logger.Log(LogLevel.Trace, $"Recall: memory after RAG access {JsonConvert.SerializeObject(_memory)}");

            var factsdata = new XmlDocument();

            factsdata.LoadXml("<facts></facts>");
            var root = factsdata.ChildNodes[0];

            foreach (var (fact, t, d) in _memory.Values.OrderBy(v => { var (f, t, d) = v; return t; }).ThenBy(v => { var (f, t, d) = v; return d; }))
            {
                var nf = factsdata.CreateElement(fact.factType!);
                if (fact.citation != null)
                    nf.SetAttribute("cit", fact.citation);
                if (fact.reference != null)
                    nf.SetAttribute("ref", fact.reference);
                if (fact.title != null)
                    nf.SetAttribute("title", fact.title);
                if (fact.content != null)
                    nf.InnerText = fact.content;
                root!.AppendChild(nf);
            }

            var msg = _history.Where(m => m.Role != Actor.UserOT && m.Role != Actor.AssistantOT).Skip(1).TakeLast(_span).ToList();

            return (factsdata, msg);
        }

        public void MarkLastHistoryMessageAsOT()
        {
            if (_history.Count > 0)
            {
                var last = _history.Last();
                if (last.Role == Actor.User)
                    last.Role = Actor.UserOT;
                else if (last.Role == Actor.Assistant)
                    last.Role = Actor.AssistantOT;
            }
        }
    }
}
