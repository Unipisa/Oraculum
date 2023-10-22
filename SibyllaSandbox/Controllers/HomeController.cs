using Microsoft.AspNetCore.Mvc;
using Oraculum;
using SibyllaSandbox.Models;
using System.Diagnostics;
using System.Text.Json;
using System.Net.Http;
using System.Text;
using static OpenAI.ObjectModels.SharedModels.IOpenAiModels;
using Lib.AspNetCore.ServerSentEvents;
using Microsoft.Extensions.Options;

namespace SibyllaSandbox.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SibyllaManager _sibyllaManager;
        private readonly IServerSentEventsService _serverSentEventsService;

        public HomeController(ILogger<HomeController> logger, SibyllaManager sibyllaManager, IServerSentEventsService serverSentEventsService)
        {
            _logger = logger;
            _sibyllaManager = sibyllaManager;
            _serverSentEventsService = serverSentEventsService;
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
                await HttpContext.Session.CommitAsync();
            }
            var sibylla = _sibyllaManager.GetSibylla("Demo", Guid.Parse(sibyllaKey));
            return sibylla;
        }

        [HttpPost]
        public async Task<string> Answer(string question)
        {
            await HttpContext.Session.LoadAsync();
            var Sibylla = await ConnectSibylla();

            var clientid = HttpContext.Session.GetString("SSEId");
            var answerid = Guid.NewGuid().ToString();
            var l = _serverSentEventsService.GetClients().Where(c => c.User == this.User).ToList();
            var t = Task.Run(() => {
                var ena = Sibylla.AnswerAsync(question);
                var en = ena.GetAsyncEnumerator();
                while (true)
                {
                    var j = en.MoveNextAsync();
                    j.AsTask().Wait();
                    if (!j.Result)
                        break;

                    if (en.Current != null)
                    {
                        var ev = new ServerSentEvent()
                        {
                            Id = answerid,
                            Data = new List<string>() { en.Current.Replace("\n", "<br/>") }
                        };
                        var jj = _serverSentEventsService.SendEventAsync(ev, c => c.Id.ToString() == clientid);
                        jj.Wait();
                    }
                }
            });
            return answerid;
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