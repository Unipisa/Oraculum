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
using WeaviateNET.Query;
using OpenAI.Interfaces;
using OpenAI.Tokenizer.GPT3;

namespace Oraculum
{
    public class SibyllaConf
    {
        public static SibyllaConf FromJson(string json)
        {
            return System.Text.Json.JsonSerializer.Deserialize<SibyllaConf>(json)!;
        }

        public string? Title { get; set; }
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

        public SibyllaConf Configuration { get { return _conf; } }

        public async Task Connect()
        {
            if (!_oraculum.IsConnected)
                await _oraculum.Connect();
        }

        public ICollection<ChatMessage> History => _chat.Messages.Where(m => m.Role == Actor.Assistant || m.Role == Actor.User).ToList();

        public async IAsyncEnumerable<string> AnswerAsync(string message, string[]? factFilter = null, string[]? categoryFilter = null, string[]? tagFilter = null)
        {
            await PrepreAnswer(message, factFilter, categoryFilter, tagFilter);

            var m = new StringBuilder();

            await foreach (var fragment in _openAiService.ChatCompletion.CreateCompletionAsStream(_chat))
            {
                if (fragment.Successful)
                {
                    var txt = fragment.Choices.First().Message.Content;
                    m.Append(txt);
                    yield return txt;
                }
            }
            if (m.Length > 0)
            {
                _chat.Messages.Add(new ChatMessage(Actor.Assistant, m.ToString()));
            }
        }

        public async Task<string?> Answer(string message, string[]? factFilter = null, string[]? categoryFilter = null, string[]? tagFilter = null)
        {
            await PrepreAnswer(message, factFilter, categoryFilter, tagFilter);

            var result = await _openAiService.ChatCompletion.CreateCompletion(_chat);
            if (result.Successful)
            {
                var ret = result.Choices.First().Message.Content;
                _chat.Messages.Add(new ChatMessage(Actor.Assistant, ret));
                return ret;
            }
            return null;
        }

        private async Task PrepreAnswer(string message, string[]? factFilter = null, string[]? categoryFilter = null, string[]? tagFilter = null)
        {
            var facts = await _oraculum.FindRelevantFacts(message, limit: 5, factTypeFilter: factFilter ?? _conf.FactFilter, categoryFilter: categoryFilter ?? _conf.CategoryFilter, tagsFilter: tagFilter ?? _conf.TagFilter);
            var newfacts = facts.Where(f => !_memory.ContainsKey(f.id!.Value)).ToList();
            if (newfacts.Count > 0)
            {
                var msg = _chat.Messages.Where(m => m.Role == Actor.System && m.Content.StartsWith("<facts>")).FirstOrDefault();
                var factsdata = new XmlDocument();

                if (msg == null) factsdata.LoadXml("<facts></facts>");
                else factsdata.LoadXml(msg.Content);

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
                if (msg == null)
                    _chat.Messages.Add(new ChatMessage(Actor.System, factsdata.OuterXml));
                else
                    msg.Content = factsdata.OuterXml;
            }
            _chat.Messages.Add(new ChatMessage(Actor.User, message));
            // add base system prompt again to make sure the assistant responds to the user correctly
            _chat.Messages.Add(new ChatMessage(Actor.System, _conf.BaseSystemPrompt!));
        }
    }
}
