using Microsoft.Extensions.DependencyInjection;
using OllamaClient;
using OllamaClient.Models;

var services = new ServiceCollection();

services.AddHttpClient<IOllamaService, OllamaService>(client =>
{
  client.BaseAddress = new Uri("http://localhost:11434");
});

var provider = services.BuildServiceProvider();

var ollama = provider.GetRequiredService<IOllamaService>();

var history = new List<OllamaMessage>
{
    new() { Role = "system", Content = "You're a helpful assistant." }
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
