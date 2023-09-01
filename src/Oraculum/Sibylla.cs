using Microsoft.Extensions.Configuration;
using OpenAI.Managers;
using OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels;
using System.Xml;

namespace Oraculum
{
    public class SibyllaConf
    {
        public string? BaseSystemPrompt { get; set; }
        public string? BaseAssistantPrompt { get; set; }
        public int MaxTokens { get; set; } = 150;
        public string Model { get; set; } = Models.Gpt_3_5_Turbo;
        public float? Temperature { get; set; } = 0.2f;
        public float? TopP { get; set; } = 1.0f;
        public float? FrequencyPenalty { get; set; } = 0.0f;
        public float? PresencePenalty { get; set; } = 0.0f;
        public string[]? FactFilter { get; set; } = null;
        public string[]? CategoryFilter { get; set; } = null;
        public string[]? TagFilter { get; set; } = null;
    }

    internal class Actor
    {
        internal const string System = "system";
        internal const string User = "user";
        internal const string Assistant = "assistant";
    }

    public class Sibylla
    {
        private Oraculum _oraculum;
        private OpenAIService _openAiService;
        private ChatCompletionCreateRequest _chat;
        private Dictionary<Guid, Fact> _memory;
        private SibyllaConf _conf;

        public Sibylla(Configuration conf, SibyllaConf sybillaConf)
        {
            _oraculum = new Oraculum(conf);
            _conf = sybillaConf;
            _openAiService = new OpenAIService(new OpenAiOptions()
            {
                ApiKey = conf.OpenAIApiKey!,
                Organization = conf.OpenAIOrgId
            });
            _chat = new ChatCompletionCreateRequest();
            _chat.MaxTokens = sybillaConf.MaxTokens;
            _chat.Temperature = sybillaConf.Temperature;
            _chat.TopP = sybillaConf.TopP;
            _chat.FrequencyPenalty = sybillaConf.FrequencyPenalty;
            _chat.PresencePenalty = sybillaConf.PresencePenalty;
            _chat.Model = sybillaConf.Model;
            _chat.Messages = new List<ChatMessage>()
            {
                new ChatMessage(Actor.System, sybillaConf.BaseSystemPrompt!),
                new ChatMessage(Actor.Assistant, sybillaConf.BaseAssistantPrompt!)
            };
            _memory = new Dictionary<Guid, Fact>();
        }

        public async Task Connect()
        {
            if (!_oraculum.IsConnected)
                await _oraculum.Connect();
        }

        public ICollection<ChatMessage> History => _chat.Messages.Where(m => m.Role == Actor.Assistant || m.Role == Actor.User).ToList();

        public async Task<string?> Answer(string message)
        {
            var facts = await _oraculum.FindRelevantFacts(message, limit: 5, factTypeFilter: _conf.FactFilter, categoryFilter: _conf.CategoryFilter, tagsFilter: _conf.TagFilter);
            var newfacts = facts.Where(f => !_memory.ContainsKey(f.id!.Value)).ToList();
            if (newfacts.Count > 0)
            {
                var factsdata = new XmlDocument();
                factsdata.LoadXml("<facts></facts>");
                var root = factsdata.ChildNodes[0];
                foreach (var f in newfacts)
                {
                    var n = factsdata.CreateElement(f.factType!);
                    if (f.citation != null)
                        n.SetAttribute("cit", f.citation);
                    if (f.reference != null)
                        n.SetAttribute("ref", f.reference);
                    if (f.title != null)
                        n.SetAttribute("title", f.title);
                    if (f.content != null)
                        n.InnerText = f.content;
                    root!.AppendChild(n);
                    _memory.Add(f.id!.Value, f);
                }
                _chat.Messages.Add(new ChatMessage(Actor.System, factsdata.OuterXml));
            }
            _chat.Messages.Add(new ChatMessage(Actor.User, message));

            var result = await _openAiService.ChatCompletion.CreateCompletion(_chat);
            if (result.Successful)
            {
                var ret = result.Choices.First().Message.Content;
                _chat.Messages.Add(new ChatMessage(Actor.Assistant, ret));
                return ret;
            }
            return null;
        }
    }
}
