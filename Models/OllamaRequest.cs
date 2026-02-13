namespace OllamaPlayground.Models;

public class OllamaRequest
{
    public string model { get; set; }
    public string prompt { get; set; }
    public bool stream { get; set; } = false;
}