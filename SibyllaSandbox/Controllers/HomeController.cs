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

        public HomeController(ILogger<HomeController> logger, SibyllaManager sibyllaManager)
        {
            _logger = logger;
            _sibyllaManager = sibyllaManager;
        }

        public async Task<IActionResult> Index()
        {
            Sibylla sibylla = await ConnectSibylla();
            return View(sibylla);
        }

        private async Task<Sibylla> ConnectSibylla()
        {
            var sibyllaKey = HttpContext.Session.GetString("sibyllaRef");
            if (sibyllaKey == null)
            {
                // It would be nice to align the expiration of the Sibylla with the expiration of the session.
                var (id, _) = await _sibyllaManager.AddSibylla("Demo", expiration: DateTime.Now.AddMinutes(60));
                HttpContext.Session.SetString("sibyllaRef", id.ToString());
                sibyllaKey = id.ToString();
            }
            var sibylla = _sibyllaManager.GetSibylla("Demo", Guid.Parse(sibyllaKey));
            return sibylla;
        }

        [HttpPost]
        public async Task<string> Answer(string question)
        {
            var Sibylla = await ConnectSibylla();
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