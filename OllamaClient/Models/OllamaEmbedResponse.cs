using System.Text.Json.Serialization;

namespace OllamaClient.Models;

public class OllamaEmbedResponse
{
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("embeddings")]
    public required List<List<float>> Embeddings { get; set; }
}
