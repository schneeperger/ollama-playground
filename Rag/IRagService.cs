namespace OllamaPlayground.Rag;

public interface IRagService
{
    Task IndexDocumentsAsync(string documentsFolder);
    Task<string> BuildContextAsync(string query, int topK = 3);
}
