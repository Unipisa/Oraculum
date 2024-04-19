using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OpenAI.ObjectModels;
using Oraculum;
using OraculumApi.Models;
using OraculumApi.Models.BackOffice;
using Fact = OraculumApi.Models.BackOffice.Fact;

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

    public async Task<string> ExtractFactsFromTextAsync(List<DataIngestTextDTO> jsonInput, string? category = null)
    {
        return await ProcessAsyncText(jsonInput, $"{BaseUrl}/API_facts_from_text", category);
    }

    public async Task<string> ExtractFactsFromWebPagesAsync(DataIngestWebPagesDTO jsonInput, string? category = null)
    {
        return await ProcessAsyncWeb(jsonInput, $"{BaseUrl}/API_facts_from_web_pages", category);
    }

    public async Task<string> ExtractFactsFromDocumentsAsync(IFormFile file, string category)
    {
        return await ProcessFileAsync(file, $"{BaseUrl}/API_facts_from_documents", category);
    }

    public async Task<string> ExtractFactsFromAudioVideoAsync(IFormFile file, string category)
    {
        return await ProcessFileAsync(file, $"{BaseUrl}/API_facts_from_AudioVideo", category);
    }

    private Task<string> ProcessAsyncText(List<DataIngestTextDTO> jsonInput, string apiUrl, string? category = null)
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
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var facts = FactConverter.ConvertToFactList(jsonResponse);
                Fact[] factsArray = facts.ToArray();
                await AddFactsToSibylla(factsArray!, category);
                _tasksStatus[taskId] = "Completed";
            }
            catch (Exception)
            {
                _tasksStatus[taskId] = "Error";
            }
        });

        return Task.FromResult(taskId);
    }

    private Task<string> ProcessAsyncWeb(DataIngestWebPagesDTO jsonInput, string apiUrl, string? category = null)
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
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var facts = FactConverter.ConvertToFactList(jsonResponse);
                Fact[] factsArray = facts.ToArray();
                await AddFactsToSibylla(factsArray, category);
                _tasksStatus[taskId] = "Completed";
            }
            catch (Exception)
            {
                _tasksStatus[taskId] = "Error";
            }
        });

        return Task.FromResult(taskId);
    }


    private Task<string> ProcessFileAsync(IFormFile file, string apiUrl, string? category = null)
    {
        string taskId = Guid.NewGuid().ToString();
        _tasksStatus.TryAdd(taskId, "Processing");

        var memoryStream = new MemoryStream();
        file.CopyTo(memoryStream);
        memoryStream.Position = 0;

        Task.Run(async () =>
        {
            try
            {
                // Initialize HttpClient with a reasonable timeout
                using var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromMinutes(_configuration.GetValue<int>("DataIngestion:MaxTimeoutMinutes"));
                // Use a separate MemoryStream for each file processing operation
                //var memoryStream = new MemoryStream();
                //await file.CopyToAsync(memoryStream);
                //memoryStream.Position = 0; // Reset position to the beginning after copying
                // Prepare MultipartFormDataContent for the request
                using var content = new MultipartFormDataContent();
                using var streamContent = new StreamContent(memoryStream);
                content.Add(streamContent, "file", file.FileName);
                // Make the HTTP request
                var response = await client.PostAsync(apiUrl, content);
                response.EnsureSuccessStatusCode();
                // Process the response
                var jsonResponse = await response.Content.ReadAsStringAsync();

                var facts = FactConverter.ConvertToFactList(jsonResponse);
                if (facts is null) throw new InvalidOperationException("Failed to deserialize response.");

                Fact[] factsArray = facts.ToArray();
                await AddFactsToSibylla(factsArray, category);

                _tasksStatus[taskId] = "Completed";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                _tasksStatus[taskId] = "Error";
                // Consider re-throwing or handling the exception based on your error handling policy
            }
        });

        return Task.FromResult(taskId); // Returns taskId immediately
    }



    public string CheckStatus(string taskId)
    {
        _tasksStatus.TryGetValue(taskId, out string? status);
        return status ?? "Not Found";
    }

    private async Task<int> AddFactsToSibylla(OraculumApi.Models.BackOffice.Fact[] facts, string? category)
    {
        var oraculumFacts = facts.Select(f => new Oraculum.Fact
        {
            id = f.Id,
            factType = f.FactType,
            category = category ?? f.Category,
            tags = f.Tags?.ToArray(),
            title = f.Title,
            content = f.Content,
            citation = f.Citation,
            reference = f.Reference,
            expiration = f.Expiration
        }).ToList();
        var addedFacts = await _sibyllaManager.AddFacts(oraculumFacts);
        if (addedFacts == 0)
        {
            throw new Exception("Failed to add facts to Sibylla");
        }
        return addedFacts;
    }
}
