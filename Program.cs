using Microsoft.Extensions.DependencyInjection;
using OllamaClient;
using OllamaClient.Models;
using OllamaPlayground.Rag;

var services = new ServiceCollection();

services.AddHttpClient<IOllamaService, OllamaService>(client =>
{
  client.BaseAddress = new Uri("http://localhost:11434");
});

services.AddSingleton<VectorStore>();
services.AddSingleton<DocumentLoader>();
services.AddSingleton<RagService>();

var provider = services.BuildServiceProvider();

var ollama = provider.GetRequiredService<IOllamaService>();
var ragService = provider.GetRequiredService<RagService>();

// Index all .txt documents on startup
var documentsFolder = Path.Combine(AppContext.BaseDirectory, "documents");

if (Directory.Exists(documentsFolder))
{
    await ragService.IndexDocumentsAsync(documentsFolder);
}
else
{
    Console.WriteLine($"[RAG] No documents folder found at {documentsFolder}. Running without RAG context.");
}

const string baseSystemPrompt =
    "You are a helpful assistant. When context from a knowledge base is provided, " +
    "use it to answer the question. If the context does not contain the answer, " +
    "say so and answer from your general knowledge.";

var history = new List<OllamaMessage>
{
    new() { Role = "system", Content = baseSystemPrompt }
};

Console.WriteLine("Chat with Ollama (type /stop to quit)\n");

while (true)
{
    Console.Write("You: ");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
        continue;

    if (input.Trim().Equals("/stop", StringComparison.OrdinalIgnoreCase))
        break;

    // Retrieve relevant context for this query and inject into system prompt
    var context = await ragService.BuildContextAsync(input, topK: 3);

    if (!string.IsNullOrEmpty(context))
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"[RAG] Context retrieved:\n{context}");
        Console.ResetColor();
    }

    history[0] = new OllamaMessage
    {
        Role = "system",
        Content = string.IsNullOrEmpty(context)
            ? baseSystemPrompt
            : $"{baseSystemPrompt}\n\n{context}"
    };

    history.Add(new OllamaMessage { Role = "user", Content = input });

    Console.Write("Assistant: ");
    var reply = new System.Text.StringBuilder();

    await foreach (var token in ollama.ChatStreamAsync(history))
    {
        Console.Write(token);
        reply.Append(token);
    }

    Console.WriteLine();

    history.Add(new OllamaMessage { Role = "assistant", Content = reply.ToString() });
}

Console.WriteLine("Goodbye!");
