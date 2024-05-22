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
using System.Reflection;

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
                    FactTypeFilter = MemoryConfiguration.FactFilter,
                    CategoryFilter = MemoryConfiguration.CategoryFilter,
                    TagsFilter = MemoryConfiguration.TagFilter,
                    Limit = MemoryConfiguration.Limit,
                    AutoCutPercentage = MemoryConfiguration.AutoCutPercentage
                };
            }
        }

        public string? Title { get; set; }
        public MemoryConfiguration MemoryConfiguration { get; set; } = new MemoryConfiguration();
        public string? BaseSystemPrompt { get; set; }
        public string? BaseAssistantPrompt { get; set; }
        public int MaxTokens { get; set; } = 150;
        public string Model { get; set; } = Models.Gpt_3_5_Turbo;
        public float? Temperature { get; set; } = 0.2f;
        public float? TopP { get; set; } = 1.0f;
        public float? FrequencyPenalty { get; set; } = 0.0f;
        public float? PresencePenalty { get; set; } = 0.0f;
        public string? OutOfScopePrefix = "*&oo&* ";
        public IList<FunctionDefinition>? FunctionsDefaultAnswerHook { get; set; } = null;
        public IList<FunctionDefinition>? FunctionsBeforeAnswerHook { get; set; } = null;
        public string? SibyllaName { get; set; }
        public bool? Hidden { get; set; }
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
        public float? AutoCutPercentage = null;
    }

    public class Sibylla
    {
        public delegate object FunctionDelegate(Dictionary<string, object> args);

        private OpenAIService _openAiService;
        private ChatCompletionCreateRequest _chat;
        private Memory _memory;
        private SibyllaConf _conf;
        private ILogger _logger;
        private Dictionary<string, FunctionDelegate> _functions = new Dictionary<string, FunctionDelegate>();
        private Dictionary<string, FunctionDelegate> _functionsBeforeAnswerHook = new Dictionary<string, FunctionDelegate>();

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

        public Sibylla(OraculumConfiguration conf, SibyllaConf sybillaConf, ILogger? logger = null)
        {
            _logger = logger ?? NullLogger.Instance;

            _conf = sybillaConf;
            _openAiService = conf.CreateOpenAIService();
            _logger.Log(LogLevel.Trace, $"Sibylla: Oraculum conf {JsonConvert.SerializeObject(conf)} Sibylla conf {JsonConvert.SerializeObject(_conf)}");
            _chat = new ChatCompletionCreateRequest();
            _chat.MaxTokens = sybillaConf.MaxTokens;
            _chat.Temperature = sybillaConf.Temperature;
            _chat.TopP = sybillaConf.TopP;
            _chat.FrequencyPenalty = sybillaConf.FrequencyPenalty;
            _chat.PresencePenalty = sybillaConf.PresencePenalty;
            _chat.Functions = sybillaConf.FunctionsDefaultAnswerHook;
            _chat.Model = sybillaConf.Model;
            _chat.Messages = new List<ChatMessage>()
            {
                new ChatMessage(Actor.System, sybillaConf.BaseSystemPrompt!),
                new ChatMessage(Actor.Assistant, sybillaConf.BaseAssistantPrompt!)
            };
            _memory = new Memory(new Oraculum(conf), _conf.MemoryConfiguration, logger: _logger);
            _memory.History.AddRange(new[]
            {
                new ChatMessage(Actor.Assistant, sybillaConf.BaseAssistantPrompt!)
            });
            // Automatically register functions from the Functions namespace
            RegisterFunctionsFromAssembly();
        }

        private void RegisterFunctionsFromAssembly()
        {
            var functionType = typeof(IFunction);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            var types = new List<Type>();

            // Fetching types from all assemblies
            foreach (var assembly in assemblies)
            {
                try
                {
                    types.AddRange(assembly.GetTypes().Where(t => functionType.IsAssignableFrom(t) && !t.IsInterface));
                }
                catch (ReflectionTypeLoadException ex)
                {
                    // Log the exceptions thrown during type loading
                    foreach (var loaderException in ex.LoaderExceptions)
                    {
                        Console.WriteLine($"Error loading types from assembly '{assembly.FullName}': {loaderException?.Message}");
                    }
                }
            }

            foreach (var type in types)
            {
                try
                {
                    if (Activator.CreateInstance(type, this) is IFunction functionInstance)
                    {
                        var isBeforeAnswerHook = _conf.FunctionsBeforeAnswerHook?.Any(f => f.Name == type.Name) ?? false;
                        var isAnswerHook = _conf.FunctionsDefaultAnswerHook?.Any(f => f.Name == type.Name) ?? false;
                        if (isBeforeAnswerHook || isAnswerHook)
                            RegisterFunction(type.Name, functionInstance.Execute, isBeforeAnswerHook);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating instance of '{type.Name}': {ex.Message}");
                }
            }
        }

        public void RegisterFunction(string name, FunctionDelegate function, bool beforeHook = false)
        {
            if (beforeHook)
            {
                _functionsBeforeAnswerHook[name] = function;
                _logger.LogInformation($"Function '{name}' added to before hook.");
                Console.WriteLine($"Function '{name}' added.");
            }
            else
            {
                _functions[name] = function;
                _logger.LogInformation($"Function '{name}' added.");
                Console.WriteLine($"Function '{name}' added.");
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
            if (_functionsBeforeAnswerHook.ContainsKey(name))
            {
                _functionsBeforeAnswerHook.Remove(name);
                _logger.LogInformation($"Function '{name}' removed from before hook.");
            }
            else _logger.LogInformation($"Function '{name}' not found.");
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
            await BeforeAnswerHook(message, filter, cancellationToken);


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
                    if (result != null && result.Choices.Count > 0)
                    {
                        var txt = result.Choices.First().Message.Content;
                        m.Append(txt);
                        yield return txt;
                    }
                }
            }
            _logger.Log(LogLevel.Trace, $"Sibylla[{Configuration.Title}|{this.GetHashCode()}]: message '{message}' with answer '{m}'");
            if (m.Length > 0)
            {
                var msg = m.ToString();
                var actor = Actor.Assistant;
                if (_conf.OutOfScopePrefix != null && msg.StartsWith(_conf.OutOfScopePrefix))
                {
                    actor = Actor.AssistantOT;
                    MarkLastHistoryMessageAsOT();
                    msg = msg.Replace(_conf.OutOfScopePrefix, "");
                }
                _memory.History.Add(new ChatMessage(actor, msg));
            }
            else
            {
                _logger.Log(LogLevel.Trace, $"Sibylla: message '{message}' with no answer");
            }
        }

        private async Task BeforeAnswerHook(string message, KnowledgeFilter? filter, CancellationToken cancellationToken)
        {
            //for each function in the before hook _functionsBeforeAnswerHook
            if (_functionsBeforeAnswerHook != null)
            {
                _chat.Functions = _conf.FunctionsBeforeAnswerHook;
                foreach (var function in _functionsBeforeAnswerHook)
                {
                    var fn = new FunctionCall();
                    _chat.FunctionCall = new Dictionary<string, string> { { "name", function.Key } };
                    await foreach (var fragment in _openAiService.ChatCompletion.CreateCompletionAsStream(_chat, cancellationToken: cancellationToken))
                    {
                        if (fragment.Successful && fragment.Choices.First().Message.FunctionCall != null)
                        {
                            fn = fragment.Choices.First().Message.FunctionCall;
                            if (fn != null)
                                break;
                        }
                    }
                    if (fn != null)
                    {
                        await foreach (var result in HandleFunctionExecution(fn, true, cancellationToken)) ;
                    }
                }
                _chat.Functions = _conf.FunctionsDefaultAnswerHook;
                // if functions empty, reset to null functionCall
                if (_chat.Functions == null)
                {
                    _chat.FunctionCall = null;
                }
            }
        }

        public async Task<string?> Answer(string message, KnowledgeFilter? filter = null)
        {
            await PrepreAnswer(message, filter);
            await BeforeAnswerHook(message, filter, default);

            var result = await _openAiService.ChatCompletion.CreateCompletion(_chat);
            if (result.Successful)
            {
                // fn
                var choice = result.Choices.First();
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
                    MarkLastHistoryMessageAsOT();
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

        private async IAsyncEnumerable<OpenAI.ObjectModels.ResponseModels.ChatCompletionCreateResponse?> HandleFunctionExecution(FunctionCall fn, bool isBeforeAnswerHook = false, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (fn == null || fn.Name == null)
            {
                yield break;
            }

            var functionArguments = fn.ParseArguments();
            var functionResult = ExecuteFunction(fn.Name, functionArguments, isBeforeAnswerHook);

            // add the result to the chat
            _chat.Messages.Add(new ChatMessage(Actor.Function, functionResult?.ToString() ?? string.Empty, fn.Name));
            _chat.FunctionCall = "auto";

            // send new completion request and yield the result
            if (!isBeforeAnswerHook)
            {
                await foreach (var result in _openAiService.ChatCompletion.CreateCompletionAsStream(_chat))
                {
                    yield return result;
                }
            }
            else
            {
                yield break;
            }

        }
        private object? ExecuteFunction(string name, Dictionary<string, object> functionArguments, bool isBeforeAnswerHook)
        {
            if (!isBeforeAnswerHook && _functions.TryGetValue(name, out var function))
            {
                return function(functionArguments);
            }
            else if (isBeforeAnswerHook && _functionsBeforeAnswerHook.TryGetValue(name, out var functionBeforeHook))
            {
                return functionBeforeHook(functionArguments);
            }
            else
            {
                _logger.Log(LogLevel.Error, $"Function '{name}' not found.");
                return null;
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

        public void MarkLastHistoryMessageAsOT()
        {
            _memory.MarkLastHistoryMessageAsOT();
        }

        // reset the memory
        public void ResetMemory()
        {
            _memory.Reset();
        }
    }
}