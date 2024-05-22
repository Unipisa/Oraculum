using OraculumApi.Models;

public interface IDataIngestionService
{
    Task<string> ExtractFactsFromTextAsync(List<DataIngestTextDTO> jsonInput, string? category = null);
    Task<string> ExtractFactsFromWebPagesAsync(DataIngestWebPagesDTO jsonInput, string? category = null);
    Task<string> ExtractFactsFromDocumentsAsync(IFormFile file, string category);
    Task<string> ExtractFactsFromAudioVideoAsync(IFormFile file, string category);
    string CheckStatus(string taskId);
}