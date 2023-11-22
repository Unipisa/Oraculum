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
using System.Reflection.Metadata;

namespace Oraculum
{
    public class SibyllaConf
    {
        public static SibyllaConf FromJson(string json)
        {
            return System.Text.Json.JsonSerializer.Deserialize<SibyllaConf>(json)!;
        }

        public KnowledgeFilter KnowledgeFilter
        {
            get
            {
                return new KnowledgeFilter()
                {
                    FactTypeFilter = FactFilter,
                    CategoryFilter = CategoryFilter,
                    TagsFilter = TagFilter
                };
            }
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
        public int FactMemoryTTL { get; set; } = 4; // 4 turns implies a maximum of 11 facts in memory
        public int MemorySpan { get; set; } = 4;
        public string? OutOfScopePrefix = "*&oo&* ";
        public IList<FunctionDefinition>? Functions { get; set; } = null;
    }

    internal class Actor
    {
        internal const string System = "system";
        internal const string User = "user";
        internal const string Assistant = "assistant";
        internal const string UserOT = "userOT";
        internal const string AssistantOT = "assistantOT";
        internal const string Function = "function";
    }

    public class KnowledgeFilter
    {
        public string[]? FactTypeFilter = null;
        public string[]? CategoryFilter = null;
        public string[]? TagsFilter = null;
        public int? Limit = 5;
    }

    public class Sibylla
    {
        private OpenAIService _openAiService;
        private ChatCompletionCreateRequest _chat;
        private Memory _memory;
        private SibyllaConf _conf;
        private ILogger _logger;
        public delegate object FunctionDelegate(Dictionary<string, object> args);
        private Dictionary<string, FunctionDelegate> _functions = new Dictionary<string, FunctionDelegate>();

        public Sibylla(Configuration conf, SibyllaConf sybillaConf, ILogger? logger = null)
        {
            _logger = logger ?? NullLogger.Instance;

            _conf = sybillaConf;
            _openAiService = conf.CreateService();
            _logger.Log(LogLevel.Trace, $"Sibylla: Oraculum conf {JsonConvert.SerializeObject(conf)} Sibylla conf {JsonConvert.SerializeObject(_conf)}");
            _chat = new ChatCompletionCreateRequest();
            _chat.MaxTokens = sybillaConf.MaxTokens;
            _chat.Temperature = sybillaConf.Temperature;
            _chat.TopP = sybillaConf.TopP;
            _chat.FrequencyPenalty = sybillaConf.FrequencyPenalty;
            _chat.PresencePenalty = sybillaConf.PresencePenalty;
            _chat.Functions = sybillaConf.Functions;
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

        public void RegisterFunction(string name, FunctionDelegate function, bool updateIfExists = true)
        {
            if (_functions.ContainsKey(name))
            {
                if (updateIfExists)
                {
                    _functions[name] = function;
                    _logger.LogInformation($"Function '{name}' updated.");
                }
            }
            else
            {
                _functions.Add(name, function);
                _logger.LogInformation($"Function '{name}' added.");
            }
        }

        // Method to unregister a function
        public void UnregisterFunction(string name)
        {
            if (_functions.ContainsKey(name))
            {
                _functions.Remove(name);
                _logger.LogInformation($"Function '{name}' removed.");
            }
        }

        public SibyllaConf Configuration { get { return _conf; } }

        public async Task Connect()
        {
            _logger.Log(LogLevel.Trace, $"Sibylla: Connect");
            if (!_memory.Oraculum.IsConnected)
                await _memory.Oraculum.Connect();
        }

        public ICollection<ChatMessage> History => _memory.History.ToList();

        public async IAsyncEnumerable<string> AnswerAsync(string message, KnowledgeFilter? filter = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await PrepreAnswer(message, filter);

            var m = new StringBuilder();
            var fn = new FunctionCall();

            await foreach (var fragment in _openAiService.ChatCompletion.CreateCompletionAsStream(_chat, cancellationToken: cancellationToken))
            {
                if (fragment.Successful && fragment.Choices.First().Message.FunctionCall != null)
                {
                    fn = fragment.Choices.First().Message.FunctionCall;
                    if (fn != null)
                        break;
                }
                if (fragment.Successful && fragment.Choices.Count > 0)
                {
                    var txt = fragment.Choices.First().Message.Content;
                    m.Append(txt);
                    yield return txt;
                }
            }
            if (fn != null)
            {
                await foreach (var result in HandleFunctionExecution(fn))
                {
                    var txt = result.Choices.First().Message.Content;
                    m.Append(txt);
                    yield return txt;
                }
            }
            _logger.Log(LogLevel.Trace, $"Sibylla: message '{message}' with answer '{m}'");
            if (m.Length > 0)
            {
                var msg = m.ToString();
                var actor = Actor.Assistant;
                if (_conf.OutOfScopePrefix != null && msg.StartsWith(_conf.OutOfScopePrefix))
                {
                    actor = Actor.AssistantOT;
                    _memory.MarkLastHistoryMessageAsOT();
                    msg = msg.Replace(_conf.OutOfScopePrefix, "");
                }
                _memory.History.Add(new ChatMessage(actor, msg));
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
                // fn
                // log full response
                Console.WriteLine($"Response:       {JsonConvert.SerializeObject(result)}");
                var choice = result.Choices.First();
                Console.WriteLine($"Message:        {choice.Message.Content}");
                var fn = choice.Message.FunctionCall;
                var msg = result.Choices.First().Message.Content;
                if (fn != null)
                {
                    var m = new StringBuilder();
                    await foreach (var functionExecResult in HandleFunctionExecution(fn))
                    {
                        var txt = functionExecResult?.Choices?.First()?.Message?.Content;
                        if (txt != null)
                        {
                            m.Append(txt);
                        }
                    }
                    msg = m.ToString();
                }
                _logger.Log(LogLevel.Trace, $"Sibylla: message '{message}' with answer '{msg}'");
                var actor = Actor.Assistant;
                if (_conf.OutOfScopePrefix != null && msg.StartsWith(_conf.OutOfScopePrefix))
                {
                    actor = Actor.AssistantOT;
                    _memory.MarkLastHistoryMessageAsOT();
                    msg.Replace(_conf.OutOfScopePrefix, "");
                }

                _memory.History.Add(new ChatMessage(actor, msg));
                return msg;
            }
            else
            {
                _logger.Log(LogLevel.Trace, $"Sibylla: message '{message}' with no answer");
            }
            return null;
        }

        private async IAsyncEnumerable<OpenAI.ObjectModels.ResponseModels.ChatCompletionCreateResponse?> HandleFunctionExecution(FunctionCall fn)
        {
            if (fn == null || fn.Name == null)
            {
                yield break;
            }
            Console.WriteLine($"Function call:  {fn.Name}");
            foreach (var entry in fn.ParseArguments())
            {
                Console.WriteLine($"  {entry.Key}: {entry.Value}");
            }

            var functionArguments = fn.ParseArguments();
            var functionResult = ExecuteFunction(fn.Name, functionArguments);

            // add the result to the chat
            _chat.Messages.Add(new ChatMessage(Actor.Function, functionResult?.ToString() ?? string.Empty, fn.Name));

            // send new completion request and yield the result
            await foreach (var result in _openAiService.ChatCompletion.CreateCompletionAsStream(_chat))
            {
                yield return result;
            }
        }
        private object ExecuteFunction(string name, Dictionary<string, object> functionArguments)
        {
            if (_functions.TryGetValue(name, out var function))
            {
                return function(functionArguments);
            }
            else
            {
                throw new ArgumentException($"Function '{name}' not recognized.");
            }
        }

        private async Task PrepreAnswer(string message, KnowledgeFilter? filter = null)
        {
            if (filter == null)
                filter = Configuration.KnowledgeFilter;

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