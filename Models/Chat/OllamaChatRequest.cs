namespace OllamaPlayground.Models.Chat;

public class OllamaChatRequest
{
    public string Model { get; set; }
    public List<OllamaMessage> Messages { get; set; }
    public bool Stream { get; set; }
}