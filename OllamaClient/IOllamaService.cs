using OllamaClient.Models;

namespace OllamaClient;

public interface IOllamaService
{
    Task<string> ChatAsync(List<OllamaMessage> messages, string? model = null);

    IAsyncEnumerable<string> ChatStreamAsync(List<OllamaMessage> messages, string? model = null, CancellationToken cancellationToken = default);

    Task<float[]> EmbedAsync(string text, string? model = null);
}
