public interface IDataIngestionService
{
    Task<string> ExtractFactsFromTextAsync(dynamic jsonInput);
    Task<string> ExtractFactsFromWebPagesAsync(dynamic jsonInput);
    Task<string> ExtractFactsFromDocumentsAsync(IFormFile file);
    Task<string> ExtractFactsFromAudioVideoAsync(IFormFile file);
    string CheckStatus(string taskId);
}