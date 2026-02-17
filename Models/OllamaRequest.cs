namespace OllamaPlayground.Models;

public class OllamaRequest
{
    public required string model { get; set; }
    public required string prompt { get; set; }
    public bool stream { get; set; } = false;
}