using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OllamaClient.Models;

namespace OllamaClient;

public class OllamaService(HttpClient http, IOptions<OllamaSettings> options) : IOllamaService
{
    private readonly OllamaSettings _settings = options.Value;

    public async Task<string> ChatAsync(List<OllamaMessage> messages, string? model = null)
    {
        var request = new OllamaChatRequest
        {
            Model = model ?? _settings.ChatModel,
            Messages = messages,
            Stream = false
        };

        var response = await http.PostAsJsonAsync("/api/chat", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Ollama chat error: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>();

        return result?.Message.Content ?? string.Empty;
    }

    public async IAsyncEnumerable<string> ChatStreamAsync(List<OllamaMessage> messages, string? model = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = new
        {
            model = model ?? _settings.ChatModel,
            messages,
            stream = true
        };

        var json = JsonSerializer.Serialize(request);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/chat") { Content = content };

        using var response = await http.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(line))
                continue;

            var chunk = JsonSerializer.Deserialize<OllamaChatResponse>(line, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (chunk?.Message?.Content is { Length: > 0 } token)
                yield return token;

            if (chunk?.Done == true)
                yield break;
        }
    }

    public async Task<float[]> EmbedAsync(string text, string? model = null)
    {
        var request = new OllamaEmbedRequest
        {
            Model = model ?? _settings.EmbedModel,
            Input = [text]
        };

        var response = await http.PostAsJsonAsync("/api/embed", request);

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
