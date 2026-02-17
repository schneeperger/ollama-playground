namespace OllamaClient.Models;

public class OllamaChatRequest
{
    public required string Model { get; set; }
    public required List<OllamaMessage> Messages { get; set; }
    public bool Stream { get; set; }
}
