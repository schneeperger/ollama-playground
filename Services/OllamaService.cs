using System.Net.Http.Json;
using System.Text.Json;
using OllamaPlayground.Models;
using OllamaPlayground.Models.Chat;

namespace OllamaPlayground.Services;

public class OllamaService : IOllamaService
{
    private readonly HttpClient _http;

    public OllamaService(HttpClient http)
    {
        _http = http;
        _http.BaseAddress = new Uri("http://localhost:11434");
    }

    // public async Task<string> AskAsync(string prompt, string model = "llama3.2")
    // {
    //     var request = new OllamaRequest
    //     {
    //         model = model,
    //         prompt = prompt,
    //         stream = false
    //     };

    //     var response = await _http.PostAsJsonAsync("/api/generate", request);

    //     if (!response.IsSuccessStatusCode)
    //     {
    //         var error = await response.Content.ReadAsStringAsync();
    //         throw new Exception($"Ollama error: {error}");
    //     }

    //     var result = await response.Content.ReadFromJsonAsync<OllamaResponse>();

    //     return result?.Response ?? string.Empty;
    // }

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

    public async IAsyncEnumerable<string> ChatStreamAsync(List<OllamaMessage> messages, string model = "llama3.2", CancellationToken cancellationToken = default)
    {
        Console.WriteLine("ay caramba");
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

            // Each line is a JSON object representing a chunk
            var chunk = JsonSerializer.Deserialize<OllamaChatResponse>(line, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            // Console.WriteLine($"DESERIALIZED: role={chunk?.Message?.Role}, content='{chunk?.Message?.Content}'");


            // Console.WriteLine($"RAW: {line}"); // <--- add this temporarily


            if (chunk?.Message?.Content is { Length: > 0 } token)
                yield return token;

            if (chunk?.Done == true)
                yield break;
        }
    }
}