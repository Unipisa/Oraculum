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
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

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
        public int FactMemoryTTL { get; set; } = 5;
        public int MemorySpan { get; set; } = 4;
        public string? OutOfScopeTag = "*&oo&*";
    }

    internal class Actor
    {
        internal const string System = "system";
        internal const string User = "user";
        internal const string Assistant = "assistant";
    }

    public class KnowledgeFilter
    {
        public string[]? FactTypeFilter = null;
        public string[]? CategoryFilter = null;
        public string[]? TagsFilter = null;
    }

    public class Sibylla
    {
        private OpenAIService _openAiService;
        private ChatCompletionCreateRequest _chat;
        private Memory _memory;
        private SibyllaConf _conf;
        private ILogger _logger;

        public Sibylla(Configuration conf, SibyllaConf sybillaConf, ILogger? logger = null)
        {
            _logger = logger ?? NullLogger.Instance;

            _conf = sybillaConf;
            _openAiService = new OpenAIService(new OpenAiOptions()
            {
                ApiKey = conf.OpenAIApiKey!,
                Organization = conf.OpenAIOrgId
            });
            _logger.Log(LogLevel.Trace, $"Sibylla: Oraculum conf {JsonConvert.SerializeObject(conf)} Sibylla conf {JsonConvert.SerializeObject(_conf)}");
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
            _memory = new Memory(new Oraculum(conf), _conf.FactMemoryTTL, logger: _logger);
            _memory.History.AddRange(new[]
            {
                new ChatMessage(Actor.Assistant, sybillaConf.BaseAssistantPrompt!)
            });
        }

        public SibyllaConf Configuration { get { return _conf; } }

        public async Task Connect()
        {
            _logger.Log(LogLevel.Trace, $"Sibylla: Connect");
            if (!_memory.Oraculum.IsConnected)
                await _memory.Oraculum.Connect();
        }

        public ICollection<ChatMessage> History => _memory.History.ToList();

        public async IAsyncEnumerable<string> AnswerAsync(string message, KnowledgeFilter? filter = null)
        {
            await PrepreAnswer(message, filter);

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
            _logger.Log(LogLevel.Trace, $"Sibylla: message '{message}' with answer '{m}'");
            if (m.Length > 0)
            {
                _chat.Messages.Add(new ChatMessage(Actor.Assistant, m.ToString()));
                _memory.History.Add(new ChatMessage(Actor.Assistant, m.ToString()));
            }
            else
            {
                _logger.Log(LogLevel.Trace, $"Sibylla: message '{message}' with no answer");
            }
        }

        public async Task<string?> Answer(string message, KnowledgeFilter? filter = null)
        {
            await PrepreAnswer(message, filter);

            var result = await _openAiService.ChatCompletion.CreateCompletion(_chat);
            if (result.Successful)
            {
                var ret = result.Choices.First().Message.Content;
                _logger.Log(LogLevel.Trace, $"Sibylla: message '{message}' with answer '{ret}'");
                _chat.Messages.Add(new ChatMessage(Actor.Assistant, ret));
                _memory.History.Add(new ChatMessage(Actor.Assistant, ret));
                return ret;
            } else {
                _logger.Log(LogLevel.Trace, $"Sibylla: message '{message}' with no answer");
            }
            return null;
        }

        private async Task PrepreAnswer(string message, KnowledgeFilter? filter = null)
        {
            if (filter == null)
                filter = new KnowledgeFilter();

            _logger.Log(LogLevel.Trace, $"PrepareAnswer: knowledge filtere is '{JsonConvert.SerializeObject(filter)}'");

            var (xml, msg) = await _memory.Recall(message, filter);
            _logger.Log(LogLevel.Trace, $"PrepareAnswer: fact xml is '{xml.OuterXml}' and messages are '{JsonConvert.SerializeObject(msg)}'");
            _chat.Messages.Clear();
            _chat.Messages.Add(new ChatMessage(Actor.System, _conf.BaseSystemPrompt!));
            _chat.Messages.Add(new ChatMessage(Actor.System, xml.OuterXml));
            _chat.Messages.Add(new ChatMessage(Actor.Assistant, _conf.BaseAssistantPrompt!));
            foreach (var m in msg)
                _chat.Messages.Add(m);
            _chat.Messages.Add(new ChatMessage(Actor.User, message));
            _memory.History.Add(new ChatMessage(Actor.User, message));
        }
    }
}
