using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OpenAI.ObjectModels;
using Oraculum;
using OraculumApi.Models.BackOffice;

public class DataIngestionService : IDataIngestionService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly string? BaseUrl;
    private readonly SibyllaManager _sibyllaManager;
    private static ConcurrentDictionary<string, string> _tasksStatus = new ConcurrentDictionary<string, string>();

    public DataIngestionService(IHttpClientFactory httpClientFactory, IConfiguration configuration, SibyllaManager sibyllaManager)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        BaseUrl = _configuration["DataIngestion:ServiceEndpoint"];
        _sibyllaManager = sibyllaManager;
    }

    public async Task<string> ExtractFactsFromTextAsync(dynamic jsonInput)
    {
        return await ProcessAsync(jsonInput, $"{BaseUrl}/API_facts_from_text");
    }

    public async Task<string> ExtractFactsFromWebPagesAsync(dynamic jsonInput)
    {
        return await ProcessAsync(jsonInput, $"{BaseUrl}/API_facts_from_web_pages");
    }

    public async Task<string> ExtractFactsFromDocumentsAsync(IFormFile file)
    {
        return await ProcessFileAsync(file, $"{BaseUrl}/API_facts_from_documents");
    }

    public async Task<string> ExtractFactsFromAudioVideoAsync(IFormFile file)
    {
        return await ProcessFileAsync(file, $"{BaseUrl}/API_facts_from_AudioVideo");
    }

    private Task<string> ProcessAsync(dynamic jsonInput, string apiUrl)
    {
        string taskId = Guid.NewGuid().ToString();
        _tasksStatus.TryAdd(taskId, "Processing");

        Task.Run(async () =>
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.PostAsJsonAsync(apiUrl, (object)jsonInput);
                response.EnsureSuccessStatusCode();
                var facts = await response.Content.ReadFromJsonAsync<OraculumApi.Models.BackOffice.Fact[]>();
                await AddFactsToSibylla(facts!);
                _tasksStatus[taskId] = "Completed";
            }
            catch (Exception)
            {
                _tasksStatus[taskId] = "Error";
            }
        });

        return Task.FromResult(taskId);
    }


    private Task<string> ProcessFileAsync(IFormFile file, string apiUrl)
    {
        string taskId = Guid.NewGuid().ToString();
        _tasksStatus.TryAdd(taskId, "Processing");

        Task.Run(async () =>
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromMinutes(_configuration.GetValue<int>("DataIngestion:TimeoutMinutes"));
                var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0; // Reset position after copying

                var content = new MultipartFormDataContent();
                var streamContent = new StreamContent(memoryStream);
                content.Add(streamContent, "file", file.FileName);

                var response = await client.PostAsync(apiUrl, content);
                response.EnsureSuccessStatusCode();

                // Reset memoryStream position if needed again here
                memoryStream.Position = 0;

                var facts = await response.Content.ReadFromJsonAsync<OraculumApi.Models.BackOffice.Fact[]>();
                await AddFactsToSibylla(facts!);
                _tasksStatus[taskId] = "Completed";
            }
            catch (Exception)
            {
                _tasksStatus[taskId] = "Error";
            }
            // Consider explicitly managing memoryStream's lifecycle if necessary
        });

        return Task.FromResult(taskId);
    }


    public string CheckStatus(string taskId)
    {
        _tasksStatus.TryGetValue(taskId, out string? status);
        return status ?? "Not Found";
    }

    private async Task<int> AddFactsToSibylla(OraculumApi.Models.BackOffice.Fact[] facts)
    {
        var oraculumFacts = facts.Select(f => new Oraculum.Fact
        {
            id = f.Id,
            factType = f.FactType,
            category = f.Category,
            tags = f.Tags?.ToArray(),
            title = f.Title,
            content = f.Content,
            citation = f.Citation,
            reference = f.Reference,
            expiration = f.Expiration
        }).ToList();

        return await _sibyllaManager.AddFacts(oraculumFacts);
    }
}
