using Microsoft.AspNetCore.Mvc;
using Oraculum;
using SibyllaSandbox.Models;
using System.Diagnostics;
using System.Text.Json;
using System.Net.Http;
using System.Text;
using static OpenAI.ObjectModels.SharedModels.IOpenAiModels;

namespace SibyllaSandbox.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SibyllaManager _sibyllaManager;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, SibyllaManager sibyllaManager, IConfiguration configuration)
        {
            _logger = logger;
            _sibyllaManager = sibyllaManager;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            Sibylla sibylla = ConnectSibylla();
            return View(sibylla);
        }

        private Sibylla ConnectSibylla()
        {
            var sibyllaKey = HttpContext.Session.GetString("sibyllaRef");
            if (sibyllaKey == null)
            {
                sibyllaKey = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("sibyllaRef", sibyllaKey);
                var oc = new Oraculum.Configuration()
                {
                    WeaviateEndpoint = _configuration["Weaviate:ServiceEndpoint"],
                    WeaviateApiKey = _configuration["Weaviate:ApiKey"],
                    OpenAIApiKey = _configuration["OpenAI:ApiKey"],
                    OpenAIOrgId = _configuration["OpenAI:OrgId"]
                };
                var sc = _configuration.GetSection("Sibylla").Get<SibyllaConf>();
                var ns = new Sibylla(oc, sc);
                ns.Connect().Wait();
                _sibyllaManager.Sibyllae.Add(sibyllaKey, ns);
            }
            var sibylla = _sibyllaManager.Sibyllae[sibyllaKey];
            return sibylla;
        }

        [HttpPost]
        public string Answer(string question)
        {
            var Sibylla = ConnectSibylla();
            var answerid = Guid.NewGuid().ToString();
            _sibyllaManager.Response.Add(answerid, new List<string>());
            var t = Task.Run(() => {
                var ena = Sibylla.AnswerAsync(question);
                var en = ena.GetAsyncEnumerator();
                while (true)
                {
                    var j = en.MoveNextAsync();
                    j.AsTask().Wait();
                    if (!j.Result)
                        break;
                    lock (_sibyllaManager.Response)
                    {
                        _sibyllaManager.Response[answerid].Add(en.Current);
                    }
                }
                lock (_sibyllaManager.Completed)
                {
                    _sibyllaManager.Completed.Add(answerid);
                }
            }
            );
            return answerid;
        }

        [HttpGet]
        public string GetAnswer(string answerid)
        {
            lock (_sibyllaManager.Response)
            {
                if (!_sibyllaManager.Response.ContainsKey(answerid))
                {
                    HttpContext.Response.StatusCode = 204;
                    return "";
                }
                var r = _sibyllaManager.Response[answerid];
                if (r.Count == 0 && _sibyllaManager.Completed.Contains(answerid))
                {
                    _sibyllaManager.Response.Remove(answerid);
                    _sibyllaManager.Completed.Remove(answerid);
                    HttpContext.Response.StatusCode = 204;
                    return "";
                }
                var ret = new StringBuilder();
                foreach (var s in r)
                {
                    ret.Append(s);
                }
                r.Clear();
                return ret.ToString();
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}