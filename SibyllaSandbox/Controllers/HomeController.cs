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
using System.Dynamic;

namespace SibyllaSandbox.Controllers
{
    public class HomeController : Controller
    {
        private const string SSEId = "SSEId";
        private const string SibyllaRef = "sibyllaRef";
        private const string DemoConf = "Demo";
        private readonly ILogger<HomeController> _logger;
        private readonly SibyllaManager _sibyllaManager;
        private readonly IServerSentEventsService _serverSentEventsService;
        private readonly IConfiguration _configuration;

        private static void SaveClientId(object? sender, ServerSentEventsClientConnectedArgs e)
        {
            e.Request.HttpContext.Session.SetString(SSEId, e.Client.Id.ToString());
            e.Request.HttpContext.Session.CommitAsync().Wait();
        }

        public HomeController(ILogger<HomeController> logger, SibyllaManager sibyllaManager, IServerSentEventsService serverSentEventsService, IConfiguration configuration)
        {
            _logger = logger;
            _sibyllaManager = sibyllaManager;
            _serverSentEventsService = serverSentEventsService;
            // to avoid multiple subscriptions I remove the previous one if exists
            _serverSentEventsService.ClientConnected -= SaveClientId;
            _serverSentEventsService.ClientConnected += SaveClientId;
            _configuration = configuration;
        }


        public async Task<IActionResult> Index()
        {
            Sibylla sibylla = await ConnectSibylla();
            return View(sibylla);
        }

        private async Task<Sibylla> ConnectSibylla()
        {
            var sibyllaKey = HttpContext.Session.GetString(SibyllaRef);
            var sibyllaName = _configuration["SibyllaConf"] ?? DemoConf;
            if (sibyllaKey == null)
            {
                // It would be nice to align the expiration of the Sibylla with the expiration of the session.
                var (id, _) = await _sibyllaManager.AddSibylla(sibyllaName, expiration: DateTime.Now.AddMinutes(60));
                HttpContext.Session.SetString(SibyllaRef, id.ToString());
                sibyllaKey = id.ToString();
                await HttpContext.Session.CommitAsync();
            }
            var sibylla = _sibyllaManager.GetSibylla(sibyllaName, Guid.Parse(sibyllaKey));
            sibylla.RegisterFunction("GetDestinationOfMission", GetDestinationOfMission);
            return sibylla;
        }

        // Example of a custom function to register in Sibylla
        private object GetDestinationOfMission(Dictionary<string, object> args)
        {
            // retuns a random destination for the mission of today based on the current date
            var date = DateTime.Now;
            var destinations = new List<string>() { "Moon", "Mars", "Jupiter", "Saturn", "Pluto" };
            var destination = destinations[date.Day % destinations.Count];
            return destination;
        }


        [HttpPost]
        public async Task<string> Answer(string question)
        {
            await HttpContext.Session.LoadAsync();
            var Sibylla = await ConnectSibylla();

            var clientid = HttpContext.Session.GetString(SSEId);
            var answerid = Guid.NewGuid().ToString();
            var l = _serverSentEventsService.GetClients().Where(c => c.User == this.User).ToList();
            var t = Task.Run(() =>
            {

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