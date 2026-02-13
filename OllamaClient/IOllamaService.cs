using OllamaClient.Models;

namespace OllamaClient;

public interface IOllamaService
{
    Task<string> ChatAsync(List<OllamaMessage> messages, string model = "llama3.2");

    IAsyncEnumerable<string> ChatStreamAsync(List<OllamaMessage> messages, string model = "llama3.2", CancellationToken cancellationToken = default);
}
