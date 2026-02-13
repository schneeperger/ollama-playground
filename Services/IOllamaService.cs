using OllamaPlayground.Models.Chat;

namespace OllamaPlayground.Services;

public interface IOllamaService
{
    // Task<string> AskAsync(string prompt, string model = "llama3.2");

    // Task<string> ChatAsync(List<OllamaMessage> messages, string model = "llama3.2");
    Task<string> ChatAsync(List<OllamaMessage> messages, string model = "llama3.2");

    IAsyncEnumerable<string> ChatStreamAsync(List<OllamaMessage> messages, string model = "llama3.2", CancellationToken cancellationToken = default);
}