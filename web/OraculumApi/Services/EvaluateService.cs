using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Oraculum;
using OraculumApi.Models;
using OraculumApi.Models.BackOffice;
using ClosedXML.Excel;
using System.IO;
using System.Linq;

public class EvaluateService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly BaseService<Metric> _metricService;
    private readonly BaseService<SibyllaPersistentConfig> _sibyllaConfigService;
    private readonly SibyllaManager _sibyllaManager;
    private static ConcurrentDictionary<string, TaskStatus> _tasksStatus = new ConcurrentDictionary<string, TaskStatus>();

    public EvaluateService(IHttpClientFactory httpClientFactory, IConfiguration configuration, SibyllaManager sibyllaManager, BaseService<SibyllaPersistentConfig> sibyllaConfigService, BaseService<Metric> metricService)
    {
        _sibyllaConfigService = sibyllaConfigService;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _sibyllaManager = sibyllaManager;
        _metricService = metricService;
    }

    private async Task<(List<string>, List<double?>)> GetContextFacts(Sibylla sibylla, string question)
    {

        var relevantFacts = await _sibyllaManager.FindRelevantFacts(
                question,
                factTypeFilter: sibylla.Configuration.MemoryConfiguration.FactFilter,
                categoryFilter: sibylla.Configuration.MemoryConfiguration.CategoryFilter,
                tagsFilter: sibylla.Configuration.MemoryConfiguration.TagFilter,
                limit: sibylla.Configuration.MemoryConfiguration.Limit,
                autoCutPercentage: sibylla.Configuration.MemoryConfiguration.AutoCutPercentage
        );

        return (relevantFacts.Select(fact => fact.content!).ToList(), relevantFacts.Select(fact => fact.distance).ToList());

    }



    public Task<string> EvaluateAsync(List<EvaluateResponseDTO> evaluateItemsDTO, string name)
    {
        string taskId = Guid.NewGuid().ToString();
        _tasksStatus.TryAdd(taskId, new TaskStatus { State = TaskState.Processing });

        Task.Run(async () =>
        {
            try
            {
                var sibyllaPersistentConfig = await _sibyllaConfigService.GetByProperty("name", name);
                var jsonPersistentConf = sibyllaPersistentConfig.Count() > 0 ? SibyllaConf.FromJson(sibyllaPersistentConfig.First().configJSON!) : null;
                if (jsonPersistentConf == null && _configuration.GetSection("DBSibyllaeConfigOnly").Get<bool>() == true)
                {
                    throw new Exception("Sibylla configuration not found in the database and DBSibyllaeConfigOnly is set to true.");
                }

                List<EvaluateItemDTO> values = new();
                List<double[]> distances = new();

                foreach (var evaluateItemDTO in evaluateItemsDTO)
                {

                    var (id, sibylla) = await _sibyllaManager.AddSibylla(name, sibyllaConf: jsonPersistentConf, expiration: DateTime.Now.AddMinutes(5));

                    var answer = await sibylla.Answer(evaluateItemDTO.question);
                    var (contents, innerDistances) = await GetContextFacts(sibylla, evaluateItemDTO.question);
                    distances.Add(innerDistances.Select(d => d ?? 0).ToArray());
                    values.Add(new EvaluateItemDTO()
                    {
                        ground_truth = evaluateItemDTO.ground_truth
                        ,
                        question = evaluateItemDTO.question,
                        answer = answer,
                        contexts = contents.ToArray(),
                    });
                    _sibyllaManager.RemoveSibylla(name, id);
                }

                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromMinutes(_configuration.GetSection("EvaluateService:MaxTimeoutMinutes").Get<int>());

                List<List<EvaluateItemDTO>> chunks = new();

                var response = await client.PostAsJsonAsync($"{_configuration["EvaluateService:ServiceEndpoint"]}/evaluate", values);
                response.EnsureSuccessStatusCode();
                var evaluateMetric = await response.Content.ReadFromJsonAsync<EvaluateMetricDTO>();

                evaluateMetric?.toMetricDTOCollection(taskId, distances).ForEach(async metricDTO => await _metricService.Add(metricDTO.toEntity()));

                _tasksStatus[taskId].State = TaskState.Completed;
                _tasksStatus[taskId].ResultId = Guid.Parse(taskId);
            }
            catch (Exception)
            {
                _tasksStatus[taskId].State = TaskState.Failed;
            }
        });

        return Task.FromResult(taskId);


    }

    public TaskStatus? CheckStatus(string taskId)
    {
        _tasksStatus.TryGetValue(taskId, out TaskStatus? status);
        return status;
    }


    public async Task<XLWorkbook> GetReport(string evaluateId, string propertyName, int limit, int offset)
    {
        var workbook = new XLWorkbook();
        try
        {
            // Ottieni i dati dal servizio Metric
            var entities = await _metricService.GetByProperty<string>(propertyName, evaluateId, limit, offset, null, null);
            var dtos = entities.Select(entity => entity.toDTO());

            var worksheet = workbook.Worksheets.Add("Data");

            var headers = typeof(MetricDTO).GetProperties().Select(p => p.Name).ToArray();
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }

            int row = 2;
            foreach (var dto in dtos)
            {
                for (int i = 0; i < headers.Length; i++)
                {
                    var property = typeof(MetricDTO).GetProperty(headers[i]);
                    var value = property.GetValue(dto, null);

                    // Se la proprietà è Contexts o Distances, converti l'array in una stringa separata da virgole
                    if (headers[i] == "Contexts")
                    {
                        value = string.Join(" *** ", ((string[])value));
                    }
                    else if (headers[i] == "Distances")
                    {
                        value = string.Join(" *** ", ((double[])value).Select(d => d.ToString()));
                    }

                    worksheet.Cell(row, i + 1).Value = value;
                }
                row++;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("An exception occurred: " + e.Message);
        }
        return workbook;
    }


}

