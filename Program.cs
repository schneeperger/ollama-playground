using Microsoft.Extensions.DependencyInjection;
using OllamaPlayground.Services;
using OllamaPlayground.Models.Chat;

var services = new ServiceCollection();

services.AddHttpClient<IOllamaService, OllamaService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:11434");
});

var provider = services.BuildServiceProvider();
var ollama = provider.GetRequiredService<IOllamaService>();
var saver = new ConversationSaverService();

var history = new List<OllamaMessage>
{
    new() { Role = "system", Content = "You are a helpful assistant." }
};

Console.WriteLine("Ollama Playground - Interactive Chat");
Console.WriteLine("Commands: /save - save conversation | /quit - exit");
Console.WriteLine(new string('-', 50));

while (true)
{
    Console.Write("\nYou: ");
    var input = Console.ReadLine()?.Trim();

    if (string.IsNullOrEmpty(input))
        continue;

    if (input.Equals("/quit", StringComparison.OrdinalIgnoreCase) ||
        input.Equals("/exit", StringComparison.OrdinalIgnoreCase))
    {
        var hasMessages = history.Exists(m => m.Role != "system");
        if (hasMessages)
        {
            Console.Write("Save conversation before exiting? (y/n): ");
            var saveChoice = Console.ReadLine()?.Trim().ToLower();
            if (saveChoice == "y" || saveChoice == "yes")
            {
                var savedPath = saver.SaveConversation(history);
                Console.WriteLine($"Conversation saved to: {savedPath}");
            }
        }
        Console.WriteLine("Goodbye!");
        break;
    }

    if (input.Equals("/save", StringComparison.OrdinalIgnoreCase))
    {
        var hasMessages = history.Exists(m => m.Role != "system");
        if (!hasMessages)
        {
            Console.WriteLine("No conversation to save yet.");
        }
        else
        {
            var savedPath = saver.SaveConversation(history);
            Console.WriteLine($"Conversation saved to: {savedPath}");
        }
        continue;
    }

    history.Add(new OllamaMessage { Role = "user", Content = input });

    Console.Write("\nAssistant: ");
    var responseBuilder = new System.Text.StringBuilder();

    await foreach (var token in ollama.ChatStreamAsync(history))
    {
        Console.Write(token);
        responseBuilder.Append(token);
        await Task.Delay(70);
    }

    Console.WriteLine();
    history.Add(new OllamaMessage { Role = "assistant", Content = responseBuilder.ToString() });
}
