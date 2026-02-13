namespace OllamaPlayground.Models;

public class OllamaResponse
{
    public string Model { get; set; }
    public string Response { get; set; }
    public bool Done { get; set; }
}