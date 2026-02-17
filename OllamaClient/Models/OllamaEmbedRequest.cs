using System.Text.Json.Serialization;

namespace OllamaClient.Models;

public class OllamaEmbedRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("input")]
    public required List<string> Input { get; set; }
}
