using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using OllamaClient.Models;

namespace OllamaClient;

public class OllamaService : IOllamaService
{
    private readonly HttpClient _http;

    public OllamaService(HttpClient http)
    {
        _http = http;
    }

    public async Task<string> ChatAsync(List<OllamaMessage> messages, string model = "llama3.2")
    {
        var request = new OllamaChatRequest
        {
            Model = model,
            Messages = messages,
            Stream = false
        };

        var response = await _http.PostAsJsonAsync("/api/chat", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Ollama chat error: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>();

        return result?.Message.Content ?? string.Empty;
    }

    public async IAsyncEnumerable<string> ChatStreamAsync(List<OllamaMessage> messages, string model = "llama3.2", [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = new
        {
            model,
            messages,
            stream = true
        };

        using var response = await _http.PostAsJsonAsync(
            "/api/chat",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            var chunk = JsonSerializer.Deserialize<OllamaChatResponse>(line, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (chunk?.Message?.Content is { Length: > 0 } token)
                yield return token;

            if (chunk?.Done == true)
                yield break;
        }
    }

    public async Task<float[]> EmbedAsync(string text, string model = "nomic-embed-text")
    {
        var request = new OllamaEmbedRequest
        {
            Model = model,
            Input = [text]
        };

        var response = await _http.PostAsJsonAsync("/api/embed", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Ollama embed error: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<OllamaEmbedResponse>();

        var vector = result?.Embeddings[0]
            ?? throw new Exception("No embedding returned from Ollama.");

        return [.. vector];
    }
}
