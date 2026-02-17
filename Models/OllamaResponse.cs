namespace OllamaPlayground.Models;

// Response DTO for the Ollama /api/generate endpoint (single-prompt, non-chat completion).
// Currently unused — kept for potential future use with the generate API.
public class OllamaResponse
{
    public required string Model { get; set; }
    public required string Response { get; set; }
    public bool Done { get; set; }
}