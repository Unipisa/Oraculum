using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.Tokenizer.GPT3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Oraculum
{
    internal class MemorySlot
    {
        internal MemorySlot(Fact fact, int ttl, DateTime lastAccess, XmlDocument factsdata)
        {
            Fact = fact;
            TTL = ttl;
            LastAccess = lastAccess;
            var nf = factsdata.CreateElement(Fact.factType!.Replace(" ", "-"));
            if (Fact.citation != null)
                nf.SetAttribute("cit", Fact.citation);
            if (Fact.reference != null)
                nf.SetAttribute("ref", Fact.reference);
            if (Fact.title != null)
                nf.SetAttribute("title", Fact.title);
            if (Fact.content != null)
                nf.InnerText = Fact.content;
            XmlFact = nf;
        }

        internal Fact Fact { get; set; }
        internal int TTL { get; set; }
        internal DateTime LastAccess { get; set; }
        internal XmlElement XmlFact { get; set; }
        internal int Tokens
        {
            get
            {
                return TokenizerGpt3.TokenCount(XmlFact.OuterXml);
            }
        }

        internal string XmlFactToString()
        {
            return XmlFact.OuterXml;
        }
    }

    public struct MemoryConfiguration
    {
        public MemoryConfiguration()
        {
        }

        public int FactMemoryTTL { get; set; } = 4; // 4 turns implies a maximum of 11 facts in memory
        public int MemorySpan { get; set; } = 4;
        public int ChatMemoryMaxContextPerc { get; set; } = 25;
        public int ContextTokens { get; set; } = 4096;
        public string[]? FactFilter { get; set; } = null;
        public int? Limit { get; set; } = 5;
        public string[]? CategoryFilter { get; set; } = null;
        public string[]? TagFilter { get; set; } = null;
        public float? AutoCutPercentage { get; set; } = 0.5f;
    }

    internal class Memory
    {
        private Dictionary<Guid, MemorySlot> _memory;
        private List<ChatMessage> _history;
        private Oraculum _oraculum;
        private MemoryConfiguration _config;
        private ILogger _logger;
        private XmlDocument _factsdata;

        internal List<ChatMessage> History => _history;

        internal Oraculum Oraculum => _oraculum;

        internal Memory(Oraculum oraculum, MemoryConfiguration memconf, ILogger? logger = null)
        {
            _logger = logger ?? NullLogger.Instance;
            _memory = new Dictionary<Guid, MemorySlot>();
            _history = new List<ChatMessage>();
            _oraculum = oraculum;
            _config = memconf;
            _factsdata = new XmlDocument();
        }

        internal async Task<(XmlDocument, List<ChatMessage>)> Recall(string message, KnowledgeFilter? filter = null)
        {
            if (filter == null)
                filter = new KnowledgeFilter();

            _logger.Log(LogLevel.Trace, $"Recall: recall from memory related to '{message}' with knowledge filter {JsonConvert.SerializeObject(filter)}");

            ForgetExpiredFacts();

            await FetchNewFacts(message, filter);

            _logger.Log(LogLevel.Trace, $"Recall: memory after RAG access {JsonConvert.SerializeObject(_memory)}");

            PrepareXmlPayload();

            var msg = SelectChatContext();

            return (_factsdata, msg);
        }

        private List<ChatMessage> SelectChatContext()
        {
            var msg = _history
                .Where(m => m.Role != Actor.UserOT && m.Role != Actor.AssistantOT)
                .Skip(1)
                .TakeLast(_config.MemorySpan)
                .ToList();

            var tokens = msg.Select(msg => TokenizerGpt3.TokenCount(msg.Content)).Sum();
            var maxtokens = (int)(_config.ContextTokens * (_config.ChatMemoryMaxContextPerc / 100.0));

            if (tokens <= maxtokens)
                return msg;

            var tokenpermsg = (int)(((double)maxtokens) / msg.Count);

            msg = msg.Select(m =>
            {
                var contentlen = TokenizerGpt3.TokenCount(m.Content);
                var newlen = Math.Min(contentlen, tokenpermsg);
                var content = m.Content;
                if (contentlen > newlen)
                {
                    var cut = (int)(content.Length * (((double)contentlen - newlen) / contentlen));
                    // Here we could trim to the last word boundary
                    content = content.Substring(0, content.Length - cut);
                }
                return new ChatMessage(m.Role, content, m.Name, m.FunctionCall);
            }).ToList();

            return msg;
        }

        private void PrepareXmlPayload()
        {
            _factsdata.LoadXml("<facts></facts>");
            var root = _factsdata.ChildNodes[0];

            foreach (var slot in _memory.Values.OrderBy(v => v.TTL).ThenBy(v => v.LastAccess))
                root!.AppendChild(slot.XmlFact);
        }

        private async Task FetchNewFacts(string message, KnowledgeFilter filter)
        {
            _logger.Log(LogLevel.Trace, $"Recall: memory after cleanup {JsonConvert.SerializeObject(_memory)}");

            var facts = await _oraculum.FindRelevantFacts(message, new FactFilter()
            {
                FactTypeFilter = filter.FactTypeFilter,
                CategoryFilter = filter.CategoryFilter,
                TagsFilter = filter.TagsFilter,
                Limit = filter.Limit,
                AutocutPercentage = filter.AutoCutPercentage
            });

            var tokens = _memory.Select(v => v.Value.Tokens).Sum();
            var maxtokens = (int)(_config.ContextTokens * ((100 - _config.ChatMemoryMaxContextPerc) / 100.0));

            var n = 0;
            foreach (var fact in facts)
            {
                if (!_memory.ContainsKey(fact.id!.Value))
                {
                    var slot = new MemorySlot(fact, Math.Max(1, _config.FactMemoryTTL - n), DateTime.Now, _factsdata);
                    if (tokens + slot.Tokens <= maxtokens)
                    {
                        _memory.Add(fact.id!.Value, slot);
                        tokens += slot.Tokens;
                    }
                    else
                    {
                        _logger.Log(LogLevel.Trace, $"Recall: memory full, skipping fact {fact.id}");
                    }
                }
                else
                {
                    var slot = _memory[fact.id!.Value];
                    slot.TTL = Math.Max(slot.TTL + 1, _config.FactMemoryTTL - n);
                }
                ++n;
            }
        }

        private void ForgetExpiredFacts()
        {
            _logger.Log(LogLevel.Trace, $"Recall: memory before {JsonConvert.SerializeObject(_memory)}");
            var toremove = new List<Guid>();
            foreach (var id in _memory.Keys)
            {
                var slot = _memory[id];
                slot.TTL--;
                if (slot.TTL <= 0)
                    toremove.Add(id);
            }

            foreach (var id in toremove)
                _memory.Remove(id);
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
        public void Reset()
        {
            _memory.Clear();
            _history.Clear();
        }
    }
}
