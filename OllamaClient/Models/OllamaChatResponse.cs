using System.Text.Json.Serialization;

namespace OllamaClient.Models;

public class OllamaChatResponse
{
    public OllamaMessage Message { get; set; }
    public bool Done { get; set; }
    public string Model { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}
